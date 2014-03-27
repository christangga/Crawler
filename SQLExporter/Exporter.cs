using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace SQLExporter {
	class Exporter {

		private MySqlConnection sql;
		private MySqlCommand command;
		private int lineNumbers;

		public Exporter() {
			sql = new MySqlConnection(@"server=localhost;database=SQL;userid=root;");
			command = sql.CreateCommand();
			sql.Open();
			StreamReader file = new StreamReader("exportline.txt");
			lineNumbers = int.Parse(file.ReadLine());
			file.Close();
		}

		public void export(){
			const int LINK = 0;
			const int TITLE = 1;
			const int WORD = 2;
			int Iline = 0;
			int part = -1;
			string line, link = "", title = "", word = "";
			StreamReader file = new StreamReader("indexing.txt");
			using(var trans = sql.BeginTransaction()) {
				using(var cmd = sql.CreateCommand()) {	
					while((line = file.ReadLine()) != null) {
						Iline++;
						if(Iline % 10000 == 0)
							Console.WriteLine(Iline + " of " + lineNumbers);
						switch(part) {
						case LINK:
							link = String.Copy(line);
							break;
						case TITLE:
							title = String.Copy(line);
							break;
						case WORD:
							if(line != "</>") {
								word = String.Copy(line);
								String addCommand = "replace into data (URL, Title, Word) values ('" + link + "', '" + title +"', '"+ word + "')";
								cmd.CommandText = addCommand;
								cmd.ExecuteNonQuery();	
							}
							break;
						default:
							break;
						}
						if(line.Equals("<LINK>"))
							part = LINK;
						else if(line.Equals("<TITLE>"))
							part = TITLE;
						else if(line.Equals("<WORD>"))
							part = WORD;
						else if(part != WORD)
							part = -1;
					}
				}
				trans.Commit();
			}
			sql.Close();
			file.Close();
		}
	}
}
