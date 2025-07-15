using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class ForkliftController : MonoBehaviour
{

    [Header("Settings")]
    public float moveSpeed = 3f;
    public float pickupHeight = 1f;
    public float pickupDuration = 1f;
    public float rotateSpeed = 90f;

    [Header("References")]
    private Transform targetObject;
    private Vector3 startPosition;
    private bool isTransporting;
    public SerialController serialController;
    [SerializeField] private Transform forkPart; // Assign in inspector!

    public bool isManualMode = false;
    public GameObject manualControlsUI;
    public Camera mainCamera;
    public Camera ForkliftCamera;
    public Transform forkliftModel; // assign this to the child named "Forklift"

    private string currentHeldDirection = null;
    private Coroutine moveCoroutine = null;
    private bool isMoving = false;

    void Update()
    {
        if (!isManualMode)
            return;

        if (currentHeldDirection == "F")
        {
            MoveForward();
            SendCommand("F");
        }
        else if (currentHeldDirection == "B")
        {
            MoveBackward();
            SendCommand("B");
        }
        else if (currentHeldDirection == "L")
        {
            RotateLeft();
            SendCommand("L");
        }
        else if (currentHeldDirection == "R")
        {
            RotateRight();
            SendCommand("R");
        }
        else
        {
            SendCommand("S");  // Stop command if no direction
        }
    }
    // Smooth continuous movement methods
    private void MoveForward()
    {
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }

    private void MoveBackward()
    {
        transform.position -= transform.forward * moveSpeed * Time.deltaTime;
    }

    private void RotateLeft()
    {
        transform.Rotate(Vector3.up, -rotateSpeed * Time.deltaTime);
    }

    private void RotateRight()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    public void SetHeldDirection(string direction)
    {
        if (!isManualMode)
        {
            Debug.LogWarning("Ignoring SetHeldDirection because not in manual mode.");
            return;
        }
        currentHeldDirection = direction;
    }

    public void StartTransportSequence(Transform item)
    {
        if (isTransporting) return;
        if (isManualMode) return;


        targetObject = item;
        startPosition = transform.position;
        isTransporting = true;

        StartCoroutine(TransportRoutine());
    }

    private IEnumerator TransportRoutine()
    {
        Debug.Log("Transporting object: " + targetObject.name + " at position: " + targetObject.position);

        // Etapa 1: Mergem la obiect

        RodeNode startNode = PathFinder.Instance.FindClosestNode(transform.position);
        if (startNode == null)
        {
            Debug.LogError("Start node is null! No nearby road node found.");
            yield break;
        }
        RodeNode endNode = PathFinder.Instance.FindClosestNode(targetObject.position);
        if (endNode == null)
        {
            Debug.LogError("End node is null! No nearby road node found.");
            yield break;
        }

        if (startNode == null || endNode == null)
        {
            Debug.LogError("No valid start or end node found.");
            yield break;
        }

        List<RodeNode> path = PathFinder.Instance.FindPath(startNode, endNode);
        if (path == null || path.Count == 0)
        {
            Debug.LogError("No path found between nodes. Aborting transport.");
            isTransporting = false;
            yield break;
        }

        foreach (RodeNode node in path)
        {
            while (Vector3.Distance(transform.position, node.transform.position) > 0.2f)
            {
                Vector3 direction = (node.transform.position - transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
                }

                transform.position = Vector3.MoveTowards(transform.position, node.transform.position, moveSpeed * Time.deltaTime);
                // Send move forward command
                SendCommand("F");
                yield return null;
            }
        }
        // Etapa 1.5: Mergem direct la obiect de la ultimul nod
    while (Vector3.Distance(transform.position, targetObject.position) > 0.2f)
        {
            Vector3 direction = (targetObject.position - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            }

            transform.position = Vector3.MoveTowards(transform.position, targetObject.position, moveSpeed * Time.deltaTime);
            SendCommand("F");
            yield return null;
        }

        // Stop movement before pickup
        SendCommand("S");

        // Etapa 2: Ridicăm obiectul

        // → Atașăm obiectul la stivuitor
        targetObject.SetParent(transform);
        targetObject.localScale = Vector3.one;

        float timer = 0;
        //Vector3 startPos = targetObject.position;
        // Vector3 pickupPos = transform.position + pickupOffset;
        

        // Schimbă pickupPos să aibă și un mic offset în față + sus
        Vector3 pickupOffset = forkPart.forward * 1.5f;

        Vector3 startForkPos = forkPart.position;
        Vector3 targetForkPos = startForkPos + pickupOffset;


        while (timer < pickupDuration)
        {
            targetObject.position = Vector3.Lerp(
                startForkPos,
                targetForkPos,
                timer / pickupDuration
            );
            timer += Time.deltaTime;

            yield return null;
        }

        // Etapa 3: Ducem obiectul la dropzone

        Transform dropZone = GameManager.Instance.dropZone;

        // 3.1: Găsim nodul cel mai apropiat de poziția curentă și cel mai apropiat de dropZone
        RodeNode currentNode = PathFinder.Instance.FindClosestNode(transform.position);
        RodeNode dropNode = PathFinder.Instance.FindClosestNode(dropZone.position);

        if (currentNode == null || dropNode == null)
        {
            Debug.LogError("No valid node found for return path to dropzone.");
            isTransporting = false;
            yield break;
        }

        // 3.2: Calculăm path-ul
        List<RodeNode> returnPath = PathFinder.Instance.FindPath(currentNode, dropNode);

        if (returnPath == null || returnPath.Count == 0)
        {
            Debug.LogError("No path found to dropzone. Aborting transport.");
            isTransporting = false;
            yield break;
        }

        // 3.3: Urmăm nodurile până la dropNode
        foreach (RodeNode node in returnPath)
        {
            // Phase 1: Rotate until aligned with the node
            while (true)
            {
                Vector3 direction = (node.transform.position - transform.position).normalized;
                float angle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);

                if (Mathf.Abs(angle) <= 5f)
                {
                    SendCommand("S");  // Stop turning
                    break;            // Aligned, exit rotation loop
                }

                // Send rotation command based on direction
                string turnCommand = angle > 0 ? "R" : "L";
                SendCommand(turnCommand);

                // Smooth rotation for simulation visuals
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

                yield return null;
            }

            // Phase 2: Move forward toward the node
            while (Vector3.Distance(transform.position, node.transform.position) > 0.2f)
            {
                Vector3 direction = (node.transform.position - transform.position).normalized;

                // Visual rotation (already aligned)
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

                transform.position = Vector3.MoveTowards(transform.position, node.transform.position, moveSpeed * Time.deltaTime);

                SendCommand("F");  // Move forward

                yield return null;
            }
        }

        // 3.4: Mergem direct la dropzone
        while (Vector3.Distance(transform.position, dropZone.position) > 0.5f)
        {
            Vector3 direction = (dropZone.position - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            }

            transform.position = Vector3.MoveTowards(transform.position, dropZone.position, moveSpeed * Time.deltaTime);
            // Send move forward command
            SendCommand("F");

            yield return null;
        }

        // Stop movement before dropping off
        SendCommand("S");

        // Finalizare
        targetObject.SetParent(null); // Eliberăm obiectul
        targetObject.position = dropZone.position;
        targetObject.rotation = Quaternion.identity; // Resetăm rotația obiectului
        GameManager.Instance.CompleteTransport();
        transform.rotation = Quaternion.identity; // Resetăm rotația stivuitorului
        isTransporting = false;


        // Etapa 4: Pas în spate
        Vector3 backwardDirection = -transform.forward;
        Vector3 targetBackPosition = transform.position + backwardDirection * 3.5f; // 3.5f = distanță în spate

        while (Vector3.Distance(transform.position, targetBackPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetBackPosition,
                moveSpeed * Time.deltaTime
            );
            // Send backward command — assuming "B" for backward, replace with your actual command if different
            SendCommand("B");
            yield return null;
        }
        // Stop after backward movement
        SendCommand("S");
    }
   public void SendCommand(string command)
    {
        if (serialController != null)
        {
            serialController.SendCommand(command);
            Debug.Log("Sent command to Arduino: " + command);
        }
    }
    public void SetManualMode(bool manual)
    {
        isManualMode = manual;
        Debug.Log("Manual mode set to: " + manual);
        if (!manual)
        {
            // When switching off manual mode, reset direction and send stop command
            currentHeldDirection = null;
            SendCommand("S");
        }
    }

    public void ToggleMode()
    {
        SetManualMode(!isManualMode);
    }

    public void ManualMove(string direction)
    {
        Debug.Log($"ManualMove called with direction {direction}. isManualMode={isManualMode}, isMoving={isMoving}");

        StartCoroutine(MoveCoroutine(direction));
        Debug.Log("ManualMove called");
        Debug.Log("Before move position: " + forkliftModel.position);
        Debug.Log("After move position: " + forkliftModel.position);

    }

    private IEnumerator MoveCoroutine(string direction)
    {
        Debug.Log("Starting MoveCoroutine with direction: " + direction);
        if (isMoving)
        {
            Debug.Log("MoveCoroutine is already running, ignoring new call");
            yield break;
        }
        isMoving = true;
        Debug.Log("Starting MoveCoroutine with direction: " + direction);

        Vector3 startPos = forkliftModel.position;
        Quaternion startRot = forkliftModel.rotation;

        Vector3 endPos = startPos;
        Quaternion endRot = startRot;

        bool doRotate = false;
        bool doMove = false;

        switch (direction)
        {
            case "F":
                doMove = true;
                break;
            case "B":
                doMove = true;
                break;
            case "L":
                doRotate = true;
                endRot *= Quaternion.Euler(0, -90f, 0);
                break;
            case "R":
                doRotate = true;
                endRot *= Quaternion.Euler(0, 90f, 0);
                break;
        }

        SendCommand(direction);

        // Rotate first, if needed
        if (doRotate)
        {
            float t = 0f;
            float duration = 0.5f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                forkliftModel.rotation = Quaternion.Slerp(startRot, endRot, t);
                Debug.Log($"Rotating... t={t}, rotation={forkliftModel.rotation.eulerAngles}");
                // Re-send rotation command every frame
                SendCommand(direction);
                yield return null;
            }
            forkliftModel.rotation = endRot;

            // Update startRot and startPos after rotation for next move step
            startRot = endRot;
            startPos = forkliftModel.position;
         
        }

        // Move forward/backward if needed, using updated forward vector
        if (doMove)
        {
            if (direction == "F")
                endPos = startPos + forkliftModel.forward * 1f;
            else if (direction == "B")
                endPos = startPos - forkliftModel.forward * 1f;

            float t = 0f;
            float duration = 0.5f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                forkliftModel.position = Vector3.Lerp(startPos, endPos, t);
                Debug.Log($"Moving... t={t}, position={forkliftModel.position}");
                yield return null;
            }
            forkliftModel.position = endPos;
        }

        isMoving = false;
    }
    private IEnumerator RepeatMove(string direction)
    {
        while (true)
        {
            yield return MoveCoroutine(direction);
            yield return new WaitForSeconds(0.05f); // Small delay to prevent flooding commands
        }
    }
}
