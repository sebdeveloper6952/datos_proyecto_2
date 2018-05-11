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

        public void AddGame(int id, string title, int platformId, string[] genres,
            string publisher)
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

        public void Dispose()
        {
            _driver?.Dispose();
        }
    }
}
