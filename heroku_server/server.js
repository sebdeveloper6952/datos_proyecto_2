var express = require('express');
var db = require('./dbApi');
var app = express();
const PORT = process.env.PORT || 5000;

// routes
app.get('/', function(req, res) {
    res.send('root directory works.');
});
app.get('/users/:username', function(req, res) {
    res.setHeader('Content-Type', 'application/json');
    var user = db.getUserByName(req.params.username, res);
});
app.get('/games/byTitle/:title', function(req, res) {
    var game = db.getGameByTitle(req.params.title, res);
});
app.get('/games/byId/:id', function (req, res) {
    var game = db.getGameById(req.params.id, res);
});
app.get('/users/recommend/byGenre/:username', function(req, res) {
    var games = db.recommendGamesByGenre(req.params.username, res);
});
app.get('/users/recommend/byPublisher/:username', function(req, res) {
    var games = db.recommendGamesByPublisher(req.params.username, res);
});
app.get('/users/recommend/byGenreAndPublisher/:username', function(req, res) {
    var games = db.recommendGamesByGenreAndPublisher(req.params.username, res);
});
app.get('/users/recommend/persons/:username', function(req, res) {
    var persons = db.recommendPersons(req.params.username, res);
});

app.listen(PORT, () => console.log(`Listening on ${ PORT }`));