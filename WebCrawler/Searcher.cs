using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler {
	class Searcher {

		public Searcher() {
			MySqlConnection sql = new MySqlConnection(@"server=localhost;database=SQL;userid=root;");
			MySqlCommand command;
			MySqlDataReader reader;
			Boolean firstWord;
			String input, query;

			sql.Open();
			command = sql.CreateCommand();

			Console.WriteLine("Masukkan Query: ");
			input = Console.ReadLine();
			while(!input.Equals("exit")) {
				firstWord = true;
				int i = 0;
				String[] listInput = input.Split(Crawler.delimiterStrings, StringSplitOptions.None);
				if(listInput.Length == 1) {
					query = "SELECT URL, Title FROM data WHERE Word LIKE '%" + listInput[0] + "%'";
					query += "GROUP BY URL ORDER BY URL ASC";
				}
				else {
					query = "SELECT a0.URL, a0.Title FROM ";
					for(int j = 1; j < listInput.Length; j++ ) {
						query += "(";
					}
					query += " ((SELECT * FROM data WHERE Word LIKE '%" + listInput[0] + "%') a" + i + ")";
					foreach(String word in listInput){
						if(firstWord){
							firstWord = false;
						}
						else {
							i++;
							query += " INNER JOIN ((SELECT b"+i+".* FROM data b"+i;
							query += " WHERE Word LIKE '%" + word + "%') a" + i;
							i++;
							query += " ) ON a0.URL=a" + (i - 1) + ".URL)";
						}
					}
					query += "GROUP BY a0.URL ORDER BY a0.URL ASC";
				}

				System.Console.WriteLine(query);
				command.CommandText = query;
				reader = command.ExecuteReader();
				while(reader.Read()) {
					System.Console.WriteLine("URL = " + reader["URL"]);
					//System.Console.WriteLine("Title = " + reader["Title"]);
				}
				reader.Close();
				System.Console.WriteLine("Masukkan Query: ");
				input = System.Console.ReadLine();
			}

			sql.Close();
		}
	}
}
