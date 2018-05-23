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
app.get('/recommendPersons', function(req, res) {
    var persons = db.recommendPersons(req.query.username, res);
});
app.post('/users', function(req, res) {
    var response = db.createUser(req.query.username, res);
});
app.post('/users/likeGame', function(req, res) {
    db.userLikesGame(req.query.username, req.query.title, res);
});
app.post('/users/likeGenre', function(req, res) {
    db.userLikesGenre(req.query.username, req.query.genre, res);
});
app.get('/recommend', function (req, res) {
    db.recommendUser(req.query.username, res);
});
app.get('/recommendFlex', function(req, res){
    db.recommendUserFlex(req.query.username, res);
});

app.listen(PORT, () => console.log(`Listening on ${ PORT }`));