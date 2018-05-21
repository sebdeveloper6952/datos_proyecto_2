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
    (person)-[:likes]->(genre:Genre), \
    (person)-[:likes]->(pub:Publisher) \
    return collect(distinct game.title) as game, \
    collect(distinct genre.name) as genre, \
    collect(distinct pub.name) as publisher',
    {username: username} 
  )
  .then(result => {
    session.close();
    var profile = {};
    profile['games'] = result.records[0]['_fields'][0];
    profile['genres'] = result.records[0]['_fields'][1];
    profile['publishers'] = result.records[0]['_fields'][2];
    res.send(JSON.stringify(profile));
  })
  .catch(error => {
    session.close();
    throw error;
  })
}

// POST /user/likesGame/:title
function userLikesGame(title, rating, res) {
  var session = driver.session();
  session.run(
  )
  .then(result => {
    session.close();
    res.send('userLikesGame: ' + rating);
  })
  .catch(error => {
    session.close();
    throw error;
  })
}

// GET /games/byId/:id
function getGameById(id, res) {
  var session = driver.session();
  session
  .run(
    'match (game:Game {id: {id}}), \
     (game)-[:hasGenre]->(gn) \
     return game.id, game.title as title, gn as genre',
    {id: parseInt(id)}
  )
  .then(result => {
    session.close();
    if(result.records.length > 0) {
      var game = {};
      game['id'] = result.records[0]['_fields'][0]['low']
      game['title'] = result.records[0]['_fields'][1];
      game['genre'] = result.records[0]['_fields'][2]['properties']['name'];
      res.send(JSON.stringify(game));
    } else res.send('Game not found');
    })
  .catch(error => {
    session.close();
    throw error;
  });
}

// GET /games/byTitle/:title
function getGameByTitle(gameTitle, res) {
    var session = driver.session();
    session
    .run(
      'match (game:Game {title: {title}}), \
       (game)-[:hasGenre]->(gn) \
       return game.id, game.title as title, gn as genre',
      {title: gameTitle}
    )
    .then(result => {
      session.close();
      if(result.records.length > 0) {
        var game = {};
        game['id'] = result.records[0]['_fields'][0]['low']
        game['title'] = result.records[0]['_fields'][1];
        game['genre'] = result.records[0]['_fields'][2]['properties']['name'];
        res.send(JSON.stringify(game));
      } else res.send('Game not found.');
      })
    .catch(error => {
      session.close();
      throw error;
    });
}

// GET /users/recommend/byGenre/:username
function recommendGamesByGenre(username, res) {
  var session = driver.session();
    session
    .run(
      'match (person:Person {username: {username}}), \
      (person)-[:likes]->(gn:Genre)<-[:hasGenre]-(game:Game) \
      return game.title as game',
      {username: username}
    )
    .then(result => {
      session.close();
      // choose 5 random games from result.records array
      var chosenGames = getRandomArrayElements(result.records, 5);
      for(i = 0; i < chosenGames.length; i++) {
        chosenGames[i] = chosenGames[i]['_fields'];
      }
      res.send(JSON.stringify(chosenGames));
      })
    .catch(error => {
      session.close();
      throw error;
    });
}

function recommendGamesByPublisher(username, res) {
  var session = driver.session();
    session
    .run(
      'match (person:Person {username: {username}}), \
      (person)-[:likes]->(pub:Publisher)<-[:publishedBy]-(game:Game) \
      return game.title as game',
      {username: username}
    )
    .then(result => {
      session.close();
      var chosenGames = getRandomArrayElements(result.records, 5);
      for(i = 0; i < chosenGames.length; i++) {
        chosenGames[i] = chosenGames[i]['_fields'];
      }
      res.send(JSON.stringify(chosenGames));
    })
    .catch(error => {
      session.close();
      throw error;
    });
}

function recommendGamesByGenreAndPublisher(username, res) {
  var session = driver.session();
    session
    .run(
      'match (person:Person {username: {username}}), \
      (person)-[:likes]->(genre:Genre), \
      (person)-[:likes]->(pub:Publisher), \
      (pub)<-[:publishedBy]-(game:Game)-[:hasGenre]->(genre) \
      return distinct game.title',
      {username: username}
    )
    .then(result => {
      session.close();
        var chosenGames = getRandomArrayElements(result.records, 5);
        for(i = 0; i < chosenGames.length; i++) {
          chosenGames[i] = chosenGames[i]['_fields'];
        }
        res.send(JSON.stringify(chosenGames));
      })
    .catch(error => {
      session.close();
      throw error;
    });
}

function recommendGamesByFriendship(username, res) {
  var session = driver.session();
    session
    .run(
      'match (person:Person {username: {username}}), \
      (person)-[:friendsWith]->(friend:Person), \
      (friend)-[r:likes]->(game:Game) where r.rating > 7 \
      return game.title',
      {title: gameTitle}
    )
    .then(result => {
      session.close();
      var chosenGames = getRandomArrayElements(result.records, 5);
      for(i = 0; i < chosenGames.length; i++) {
        chosenGames[i] = chosenGames[i]['_fields'];
      }
      res.send(JSON.stringify(chosenGames));
    })
    .catch(error => {
      session.close();
      throw error;
    });
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
exports.getGameById = getGameById;
exports.getGameByTitle = getGameByTitle;
exports.getUserByName = getUserByName;
//exports.userLikesGame = userLikesGame;
exports.recommendGamesByGenre = recommendGamesByGenre;
exports.recommendGamesByPublisher = recommendGamesByPublisher;
exports.recommendGamesByGenreAndPublisher = recommendGamesByGenreAndPublisher;
exports.recommendGamesByFriendship = recommendGamesByFriendship;
exports.recommendPersons = recommendPersons;