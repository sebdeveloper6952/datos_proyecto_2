var neo4j = require('neo4j-driver').v1;
var graphenedbURL = process.env.GRAPHENEDB_BOLT_URL;
var graphenedbUser = process.env.GRAPHENEDB_BOLT_USER;
var graphenedbPass = process.env.GRAPHENEDB_BOLT_PASSWORD;
var driver = neo4j.driver(graphenedbURL, neo4j.auth.basic(graphenedbUser, graphenedbPass));

// GET /users/:username
function getUserByName(username, res) {
  var session = driver.session();
  session.run(
    'match (person:Person {username: {username}}), \
    (person)-[:likes]->(game:Game), \
    (person)-[:likes]->(genre:Genre) \
    return collect(distinct game.title) as game, \
    collect(distinct genre.name) as genre',
    {username: username} 
  )
  .then(result => {
    session.close();
    var profile = {};
    profile['games'] = result.records[0]['_fields'][0];
    profile['genres'] = result.records[0]['_fields'][1];
    res.send(JSON.stringify(profile));
  })
  .catch(error => {
    session.close();
    throw error;
  })
}

// POST /users/likeGame
function userLikesGame(username, title, res) {
  var session = driver.session();
  session.run(
    'match (user:Person {username:{username}}), \
    (game:Game {title:{title}}) \
    create (user)-[:likes]->(game) return user.username',
    {username:username, title:title}
  )
  .then(result => {
    session.close();
    // response
    res.status(201).send('Relationship created successfully.');
  })
  .catch(error => {
    session.close();
    throw error;
  })
}

// POST /users/:username
function createUser(username, res) {
  var session = driver.session();
  session
  .run(
    'create (p:Person {username:{username}}) return p.username as username',
    {username: username}
  )
  .then(result => {
    session.close();
    res.status(201).send('User created successfully.');
  })
  .catch(error => {
    session.close();
    throw error;
  });
}

// POST /users/likeGenre
function userLikesGenre(username, genre, res) {
  var session = driver.session();
  session.run(
    'match (user:Person {username:{username}}), \
    (genre:Genre {name:{genre}}) \
    create (user)-[:likes]->(genre) return user.username',
    {username:username, genre:genre}
  )
  .then(result => {
    session.close();
    // response
    res.status(201).send('Relationship created successfully.');
  })
  .catch(error => {
    session.close();
    throw error;
  })
}

function recommendUser(username, res) {
  var session = driver.session();
  session.run(
    'match (user:Person {username:{username}}), \
      (user)-[:likes]->(g1:Game), \
      (user)-[:likes]->(gn:Genre), \
      (user)-[:likes]->(pub:Publisher), \
      (pub)<-[:publishedBy]-(g2)-[:hasGenre]->(gn) \
      return distinct g2.title as title', 
      {username:username}
  )
  .then(result => {
    session.close();
    var chosenGames = getRandomArrayElements(result.records, 10);
    for(i = 0; i < chosenGames.length; i++) {
      chosenGames[i] = chosenGames[i]['_fields'];
    }
    res.send(JSON.stringify(chosenGames));
  })
  .catch(error => {
    session.close();
    throw error;
  })
}

function recommendUserFlex(username, res) {
  var session = driver.session();
  session.run(
    'match (user:Person {username:{username}}), \
      (user)-[:likes]->(g1:Game), \
      (user)-[:likes]->(gn:Genre), \
      (g2)-[:hasGenre]->(gn) \
      return distinct g2.title as title', 
      {username:username}
  )
  .then(result => {
    session.close();
    var chosenGames = getRandomArrayElements(result.records, 10);
    for(i = 0; i < chosenGames.length; i++) {
      chosenGames[i] = chosenGames[i]['_fields'];
    }
    res.send(JSON.stringify(chosenGames));
  })
  .catch(error => {
    session.close();
    throw error;
  })
}

function recommendPersons(username, res) {
  var session = driver.session();
    session
    .run(
      'match (person:Person {username: {username}}), \
      (person)-[:likes]->(gn:Genre)<-[:likes]-(s:Person) \
      where not (person)-[:friendsWith]->(s) \
      return distinct s.username',
      {username: username}
    )
    .then(result => {
      session.close();
      var persons = getRandomArrayElements(result.records, 5);
      for(i = 0; i < persons.length; i++) {
        persons[i] = persons[i]['_fields'];
      }
      res.send(JSON.stringify(persons));
      })
    .catch(error => {
      session.close();
      throw error;
    });
}

function getRandomArrayElements(array, count) {
  if(array.length < count) return array;
  var chosenGames = [];
  while(chosenGames.length < 5)
  {
    randomIndex = Math.floor(Math.random() * array.length);
    if(!chosenGames.includes(array[randomIndex]))
      chosenGames.push(array[randomIndex]);
  }
  return chosenGames;
}

// make methods available for require
exports.createUser = createUser;
exports.getUserByName = getUserByName;
exports.userLikesGame = userLikesGame;
exports.userLikesGenre = userLikesGenre;
exports.recommendUser = recommendUser;
exports.recommendUserFlex = recommendUserFlex;
exports.recommendPersons = recommendPersons;