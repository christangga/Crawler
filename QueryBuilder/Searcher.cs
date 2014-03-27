using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryBuilder {
	class Searcher {

		private string[] delimiterStrings = { " ", ",", ".", ":", "\t", "\n", "?", "\\", "/", "!", "@", "\r", "(", ")", "\"", "\'", "<", ">", "[", "]", "{", "}", "|", "”", "“", "~", "#", "&", "-", ";" };
		
		Boolean firstWord;
		String input, query;
		StreamWriter swQuery;
		StreamReader rawQuery;

		public Searcher() {
			firstWord = true;
			swQuery = new StreamWriter("query.txt");
			rawQuery = new StreamReader("raw.txt");
		}

		public void search() {
			input = rawQuery.ReadLine();
			
			int i = 0;
			String[] listInput = input.Split(delimiterStrings, System.StringSplitOptions.RemoveEmptyEntries);
			if(listInput.Length == 1) {
				query = "SELECT URL, Title FROM data WHERE Word LIKE '%" + listInput[0] + "%'";
				query += "GROUP BY URL ORDER BY URL ASC";
			}
			else {
				query = "SELECT DISTINCT * FROM ";
				for(int j = 1; j < listInput.Length; j++ ) {
					query += "(";
				}
				query += " ((SELECT URL,Title FROM data WHERE Word LIKE '%" + listInput[0] + "%') a" + i + ")";
				foreach(String word in listInput){
					if(firstWord){
						firstWord = false;
					}
					else {
						i++;
						query += " NATURAL JOIN ((SELECT b"+i+".URL, b"+i+".Title FROM data b"+i;
						query += " WHERE Word LIKE '%" + word + "%') a" + i + "))";
						i++;
					}
				}
				//query += "GROUP BY URL ORDER BY URL ASC";
			}

			swQuery.WriteLine(query);
			swQuery.Close();
		}
	}
}
