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

        public MainWindow()
        {
            InitializeComponent();
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
                string line = sr.ReadLine();
                Dictionary<string, string> map = new Dictionary<string, string>();
                while(line != null)
                {
                    string[] lineParts = line.Split(':');
                    map.Add(lineParts[0], lineParts[1]);
                    line = sr.ReadLine();
                    // write game to db
                    if (line == GAME_DATA_DELIMITER)
                    {
                        // advance to next line
                        line = sr.ReadLine();
                        // write game to db
                        string id = string.Empty;
                        string title = string.Empty;
                        string platformId = string.Empty;
                        string genres = string.Empty;
                        string publisher = string.Empty;
                        map.TryGetValue("id", out id);
                        map.TryGetValue("GameTitle", out title);
                        map.TryGetValue("PlatformId", out platformId);
                        map.TryGetValue("Genres", out genres);
                        map.TryGetValue("Publisher", out publisher);
                        string[] gArr = null;
                        if(genres != null) gArr = genres.Split(',');
                        neo4jManager.instance.AddGame(int.Parse(id), title, int.Parse(platformId),
                            gArr, publisher);
                        map.Clear();
                    }
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // close connection to db
            neo4jManager.instance.Dispose();
        }
    }
}
