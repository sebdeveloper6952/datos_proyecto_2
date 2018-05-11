const express = require('express');
const PORT = process.env.PORT || 5000;
const app = express();

app.get('/games/:id', (req, res) => {
    res.send('Asked for game: ' + req.params.id);
});

app.listen(PORT, () => console.log(`Listening on ${ PORT }`));