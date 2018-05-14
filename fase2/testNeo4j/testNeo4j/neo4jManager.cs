using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4j.Driver.V1;

namespace testNeo4j
{
    public class neo4jManager : IDisposable
    {
        public static neo4jManager instance = new neo4jManager("bolt://localhost:7687", "neo4j", "pass123");

        private readonly IDriver _driver;

        private neo4jManager(string uri, string user, string password)
        {
            _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        }

        public void AddUser(string name)
        {
            using (var session = _driver.Session())
            {
                var person = session.WriteTransaction(tx =>
                {
                    var result = tx.Run("CREATE (a:Person) " +
                                        "SET a.name = $name " +
                                        "RETURN a.name",
                        new { name });
                    return result.Single()[0].As<string>();
                });
                Console.WriteLine(person);
            }
        }

        public void AddPlatform(int id, string name)
        {
            using (var session = _driver.Session())
            {
                session.WriteTransaction(tx =>
                {
                    var result = tx.Run("CREATE (p:Platform {id: $id, name: $name})",
                        new { id, name });
                });
            }
        }

        public void AddGenre(int id, string name)
        {
            using (var session = _driver.Session())
            {
                session.WriteTransaction(tx =>
                {
                    var result = tx.Run("CREATE (gn:Genre {id: $id, name: $name})",
                        new { id, name });
                });
            }
        }

        public void AddGame(int id, string title)
        {
            using (var session = _driver.Session())
            {
                session.WriteTransaction(tx =>
                {
                    var result = tx.Run("CREATE (g:Game {id: $id, title: $title})",
                        new { id, title });
                });
            }
        }

        public void AddPublisher(int id, string name)
        {
            using (var session = _driver.Session())
            {
                session.WriteTransaction(tx =>
                {
                    var result = tx.Run("CREATE (p:Publisher {id: $id, name: $name})",
                        new { id, name });
                });
            }
        }

        public void ConnectGameToGenre(int gameId, string genreName)
        {
            using (var session = _driver.Session())
            {
                session.WriteTransaction(tx =>
                {
                    var result = tx.Run("MATCH (g:Game {id: $gameId})," +
                                            "(gn:Genre {name: $genreName})" +
                                            "CREATE (g)-[:hasGenre]->(gn)",
                            new { gameId, genreName });
                });
            }
        }

        public void ConnectGameToPlatform(int gameId, int platformId)
        {
            using (var session = _driver.Session())
            {
                session.WriteTransaction(tx =>
                {
                    var result = tx.Run("MATCH (g:Game {id: $gameId})," +
                                            "(p:Platform {id: $platformId})" +
                                            "CREATE (g)-[:availableIn]->(p)",
                            new { gameId, platformId });
                });
            }
        }

        public void ConnectGameToPublisher(int gameId, string publisher)
        {
            using (var session = _driver.Session())
            {
                session.WriteTransaction(tx =>
                {
                    var result = tx.Run("MATCH (g:Game {id: $gameId})," +
                                            "(pub:Publisher {name: $publisher})" +
                                            "CREATE (g)-[:publishedBy]->(pub)",
                            new { gameId, publisher });
                });
            }
        }

        public void ConnectUserToGenre(string username, int genreId)
        {
            using (var session = _driver.Session())
            {
                session.WriteTransaction(tx =>
                {
                    var result = tx.Run("MATCH (p:Person {username: $username})," +
                                            "(g:Genre {id: $genreId})" +
                                            "CREATE (p)-[:likes]->(g)",
                            new { username, genreId});
                });
            }
        }

        public void ConnectUserToPlatform(string username, int platformId)
        {
            using (var session = _driver.Session())
            {
                session.WriteTransaction(tx =>
                {
                    var result = tx.Run("MATCH (p:Person {username: $username})," +
                                            "(pl:Platform {id: $platformId})" +
                                            "CREATE (p)-[:playsIn]->(pl)",
                            new { username, platformId});
                });
            }
        }

        public void ConnectUserToPublisher(string username, int pubId)
        {
            using (var session = _driver.Session())
            {
                session.WriteTransaction(tx =>
                {
                    var result = tx.Run("MATCH (p:Person {username: $username})," +
                                            "(pub:Publisher {id: $pubId})" +
                                            "CREATE (p)-[:likes]->(pub)",
                            new { username, pubId});
                });
            }
        }

        public void ConnectUserToGame(string username, int gameId, int rating)
        {
            using (var session = _driver.Session())
            {
                session.WriteTransaction(tx =>
                {
                    var result = tx.Run("MATCH (p:Person {username: $username})," +
                                            "(g:Game {id: $gameId})" +
                                            "CREATE (p)-[:likes {rating: $rating}]->(g)",
                            new { username, gameId, rating });
                });
            }
        }

        public void AddRandomUser(string username)
        {
            using (var session = _driver.Session())
            {
                session.WriteTransaction(tx =>
                {
                    tx.Run("CREATE (p:Person {username: $username})",
                        new { username });
                });
            }
        }

        public void Dispose()
        {
            _driver?.Dispose();
        }
    }
}
