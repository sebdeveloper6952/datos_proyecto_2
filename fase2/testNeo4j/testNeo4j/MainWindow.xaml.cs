using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace testNeo4j
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        protected const string GAMES_FILE = "clean_games.txt";
        protected const string PLATFORMS_FILE = "platforms_list.xml";
        protected const string GENRES_FILE = "genres_list.txt";
        protected const string PUBLISHERS_FILE = "publishers_list.txt";
        protected const string GAME_DATA_DELIMITER = "--game--data--finished--";
        protected int nextUserId;

        public MainWindow()
        {
            InitializeComponent();
            nextUserId = 1;
        }

        private void btn_LoadPlatforms_Click(object sender, RoutedEventArgs e)
        {
            int numberOfPlatforms = 0;
            using (StreamReader sr = File.OpenText(PLATFORMS_FILE))
            {
                string line = sr.ReadLine();
                string prevLine = string.Empty;
                while(line != null)
                {
                    string[] parts = line.Split('<', '>');
                    if(parts.Length > 2)
                    {
                        if(parts[1].Equals("name"))
                        {
                            numberOfPlatforms++;
                            string[] prevParts = prevLine.Split('<', '>');
                            neo4jManager.instance.AddPlatform(int.Parse(prevParts[2]), parts[2]);
                            Console.WriteLine(prevParts[2] + "," + parts[2]);
                        }
                    }
                    // save previous line and advance to next line
                    prevLine = line;
                    line = sr.ReadLine();
                }
            }
            MessageBox.Show("Finished writing to db...");
        }

        private void btn_LoadGames_Click(object sender, RoutedEventArgs e)
        {
            using (StreamReader sr = File.OpenText(GAMES_FILE))
            {
                Dictionary<string, string> map = new Dictionary<string, string>();
                string line = sr.ReadLine();
                string gameTitle = string.Empty;
                int id = 0;
                while(line != null)
                {
                    string[] parts = line.Split(':');
                    if(parts.Length > 1)
                    {
                        if(parts[0].Equals("id"))
                        {
                            id = int.Parse(parts[1]);
                        }
                        else if(parts[0].Equals("GameTitle"))
                        {
                            gameTitle += parts[1];
                            for (int i = 2; i < parts.Length; i++)
                                gameTitle += parts[i];
                            // write game to db
                            neo4jManager.instance.AddGame(id, gameTitle);
                            // reset game data variables
                            gameTitle = string.Empty;
                            id = 0;
                        }
                    }
                    line = sr.ReadLine();
                }
            }
            MessageBox.Show("Finished writing games to db...");
        }

        private void btn_LoadGenres_Click(object sender, RoutedEventArgs e)
        {
            using (StreamReader sr = File.OpenText(GENRES_FILE))
            {
                List<string> genres = new List<string>();
                string line = sr.ReadLine();
                while(line != null)
                {
                    if (!genres.Contains(line)) genres.Add(line);
                    line = sr.ReadLine();
                }
                int id = 0;
                foreach (string genre in genres)
                {
                    neo4jManager.instance.AddGenre(id++, genre);
                }
            }
            MessageBox.Show("Finished writing genres...");
        }

        private void btn_LoadPublishers_Click(object sender, RoutedEventArgs e)
        {
            using (StreamReader sr = File.OpenText(PUBLISHERS_FILE))
            {
                string line = sr.ReadLine();
                int id = 1;
                while(line != null)
                {
                    neo4jManager.instance.AddPublisher(id++, line);
                    line = sr.ReadLine();
                }
            }
            MessageBox.Show("Finished writing to db...");
        }

        private void btn_BuildRelationships_Click(object sender, RoutedEventArgs e)
        {
            // iterate through games file
            // foreach game, store genres in array
            // pass to neo4jManager game id and genres array
            using (StreamReader sr = File.OpenText(GAMES_FILE))
            {
                string line = sr.ReadLine();
                int gameId = 0;
                string[] genres = null;
                while(line != null)
                {
                    string[] lineParts = line.Split(':');
                    if(lineParts.Length > 1)
                    {
                        if(lineParts[0].Equals("id"))
                        {
                            gameId = int.Parse(lineParts[1]);
                        }
                        else if(lineParts[0].Equals("Genres"))
                        {
                            genres = lineParts[1].Split(',');
                            foreach(string genre in genres)
                                neo4jManager.instance.ConnectGameToGenre(gameId, genre);
                        }
                    }
                    line = sr.ReadLine();
                }
            }
            MessageBox.Show("Finished building relations...");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // close connection to db
            neo4jManager.instance.Dispose();
        }

        private void btn_ConnectGamesPlatforms_Click(object sender, RoutedEventArgs e)
        {
            using (StreamReader sr = File.OpenText(GAMES_FILE))
            {
                string line = sr.ReadLine();
                int gameId = 0;
                int platformId = 0;
                while (line != null)
                {
                    string[] parts = line.Split(':');
                    if(parts.Length > 1)
                    {
                        if (parts[0].Equals("id"))
                            gameId = int.Parse(parts[1]);
                        else if(parts[0].Equals("PlatformId"))
                        {
                            platformId = int.Parse(parts[1]);
                            neo4jManager.instance.ConnectGameToPlatform(gameId, platformId);
                        }

                    }
                    line = sr.ReadLine();
                }
            }
            MessageBox.Show("Finished building relations...");
        }

        private void btn_ConnectGamesPublishers_Click(object sender, RoutedEventArgs e)
        {
            using (StreamReader sr = File.OpenText(GAMES_FILE))
            {
                string line = sr.ReadLine();
                int gameId = 0;
                string pubName = string.Empty;
                while (line != null)
                {
                    string[] parts = line.Split(':');
                    if (parts.Length > 1)
                    {
                        if (parts[0].Equals("id"))
                            gameId = int.Parse(parts[1]);
                        else if (parts[0].Equals("Publisher"))
                        {
                            pubName = parts[1];
                            neo4jManager.instance.ConnectGameToPublisher(gameId, pubName);
                        }

                    }
                    line = sr.ReadLine();
                }
            }
            MessageBox.Show("Finished building relations...");
        }

        private void btn_RandomUser_Click(object sender, RoutedEventArgs e)
        {
            neo4jManager.instance.AddRandomUser(string.Concat("Person", nextUserId++));
            MessageBox.Show("Added user...");
        }

        private void btn_ConnectUsersGenres_Click(object sender, RoutedEventArgs e)
        {
            // for each user, make him like 5 random genres
            Random r = new Random();
            for(int i = 1; i < 10; i++)
            {
                for(int j = 0; j < 5; j++)
                {
                    neo4jManager.instance.ConnectUserToGenre(string.Concat("Person", i), 
                        r.Next(19) + 1);
                }
            }
            MessageBox.Show("Connected users to genres...");
        }

        private void btn_ConnectUsersPlatforms_Click(object sender, RoutedEventArgs e)
        {
            // there are 109 platforms
            // each user plays in 3 random platforms
            Random r = new Random();
            for(int i = 1; i <= 10; i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    neo4jManager.instance.ConnectUserToPlatform(string.Concat("Person", i),
                        r.Next(109)+1);
                }
            }
            MessageBox.Show("Connected users to platforms...");
        }

        private void btn_ConnectUsersPublishers_Click(object sender, RoutedEventArgs e)
        {
            // there are 5487 publishers
            // each user likes in 5 random publishers
            Random r = new Random();
            for (int i = 1; i <= 10; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    neo4jManager.instance.ConnectUserToPublisher(string.Concat("Person", i),
                        r.Next(5487) + 1);
                }
            }
            MessageBox.Show("Connected users to publishers...");
        }

        private void btn_ConnectUsersGames_Click(object sender, RoutedEventArgs e)
        {
            // there are 45576 games
            // each user plays in 10 random games
            Random r = new Random();
            for (int i = 1; i <= 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    neo4jManager.instance.ConnectUserToGame(string.Concat("Person", i),
                        r.Next(45576) + 1, r.Next(10)+1);
                }
            }
            MessageBox.Show("Connected users to games...");
        }
    }
}
