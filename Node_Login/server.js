const express = require('express');
const { Pool } = require('pg');
const cors = require('cors');

const app = express();
const port = 3000;

app.use(cors());
app.use(express.json());

// Setup PostgreSQL connection pool
const pool = new Pool({
  user: 'uidualc',
  host: 'localhost',
  database: 'inventory_db',
  password: 'Claudiu1',
  port: 5432,
});

// Get slot info
app.get('/slot/:slotId', async (req, res) => {
  const slotId = parseInt(req.params.slotId);
  try {
    const result = await pool.query('SELECT * FROM slots WHERE slot_id = $1', [slotId]);
    if (result.rows.length === 0) {
      return res.status(404).json({ error: 'Slot not found' });
    }
    res.json(result.rows[0]);
  } catch (err) {
    console.error(err);
    res.status(500).send('Server error');
  }
});

// Update slot info
app.post('/slot/update', async (req, res) => {
  const { slot_id, item_name, quantity_kg, pos_x, pos_y, pos_z, deleted } = req.body;
  try {
    await pool.query(
      `UPDATE slots SET item_name=$1, quantity_kg=$2, pos_x=$3, pos_y=$4, pos_z=$5, deleted=$6 WHERE slot_id=$7`,
      [item_name, quantity_kg, pos_x, pos_y, pos_z, deleted, slot_id]
    );
    res.json({ status: 'ok' });
  } catch (err) {
    console.error(err);
    res.status(500).send('Server error');
  }
});

app.get('/slots', async (req, res) => {
  try {
    const result = await pool.query('SELECT * FROM slots WHERE delete = false ORDER BY slot_id');
    res.json(result.rows);
  } catch (err) {
    console.error(err);
    res.status(500).send('Server error');
  }
});

app.listen(port, () => {
  console.log(`Server running on http://localhost:${port}`);
});