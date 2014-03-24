using HtmlAgilityPack;
using System.Data.SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace WebCrawler {

	class Crawler {
		private const int BFS_MAX_DEPTH = 2;
		private const int DFS_MAX_DEPTH = 2;

		private char[] delimiterChars = {' ', ',', '.', ':', '\t', '\n', '?', '\\', '/', '!', '@', '\r', '(', ')', '\'', '\"', '<', '>', '[', ']', '{', '}', '|', '”', '“', '~'};
		private LinkedList<String> urlsToVisit;
		private HashSet<String> urlsVisited;
		private StreamWriter swCrawl, swIndex;

		SQLiteConnection sql;
		SQLiteCommand command;
		
		public Crawler(String baseUrl, int searchtype) {
			urlsToVisit = new LinkedList<String>();
			urlsVisited = new HashSet<String>();
			swCrawl = new StreamWriter("crawling.txt");
			swIndex = new StreamWriter("indexing.txt");
			//SQLiteConnection.CreateFile("SQL.sqlite");
			sql = new SQLiteConnection("Data Source=SQL.sqlite;Version=3;");
			sql.Open();
			String createDB = "create table if not exists data (URL varchar(200), Title varchar(200), Word varchar(100), UNIQUE (URL, Title, Word) on conflict replace)";
			command = new SQLiteCommand(sql);
			command.CommandText = createDB;
			command.ExecuteNonQuery();
			Console.WriteLine("Initializing Crawler . . .");
			if (searchtype == 0) {
				//crawlBFS(baseUrl);
			} else if (searchtype == 1) {
				//crawlDFS(baseUrl, 0);
			}
			swCrawl.Close();
			swIndex.Close();
			//writeToDB();
			Console.WriteLine("Done Crawling . . .");
			search();
			sql.Close();
			
		}

		private void addUrl(String urlpath) {
			urlsToVisit.AddLast(urlpath);
			urlsVisited.Add(urlpath);
		}

		private String getHtmlText(String urlpath) {
			try {
				HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create(urlpath);
				request.UserAgent = "Crawler";
				request.Timeout = 1000;
				WebResponse response = request.GetResponse();
				Stream stream = response.GetResponseStream();
				StreamReader reader = new StreamReader(stream);
				String htmlText = reader.ReadToEnd();
				return htmlText;
			} catch (Exception e) {
				String s = e.ToString();
				Console.WriteLine(s);
			}
			return null;
		}

		private void crawlBFS(String baseUrl) {
			swCrawl.WriteLine(">>>>>>>>>> BFS LEVEL 0 <<<<<<<<<<\n");
			addUrl(baseUrl);
			swCrawl.WriteLine(urlsVisited.Count + " " + baseUrl + "\n");
			for (int i = 1; i <= BFS_MAX_DEPTH; ++i) {
				LinkedList<String> newUrl = new LinkedList<String>();

				swCrawl.WriteLine(">>>>>>>>>> BFS LEVEL " + i + " <<<<<<<<<<\n");
				foreach (String urlpath in urlsToVisit) {
					try {
						String htmlText = getHtmlText(urlpath);
						swIndex.WriteLine("<LINK>");
						swIndex.WriteLine(urlpath);
						swIndex.WriteLine("</>");
						if (htmlText != null) {
							HtmlDocument doc = new HtmlDocument();
							doc.LoadHtml(htmlText);

							// crawling
							foreach (HtmlNode attNode in doc.DocumentNode.SelectNodes("//a")) {
								String url = attNode.GetAttributeValue("href", null);
								if (url != null) {
									if (isUrlIgnored(url)) {
										continue;
									} else {
										url = new Uri(new Uri(urlpath), url).AbsoluteUri.ToString();
									}
									if (!urlsVisited.Contains(url)) {
										newUrl.AddLast(url);
										urlsVisited.Add(url);
										Console.WriteLine(urlsVisited.Count);
										swCrawl.WriteLine(urlsVisited.Count + " " + urlpath + " " + url);
									}
								}
							}
							HtmlNodeCollection divNodes = doc.DocumentNode.SelectNodes("//div");
							HtmlNodeCollection comments = doc.DocumentNode.SelectNodes("//comment()");

							if (comments != null) {
								foreach (HtmlNode comment in comments) {
									comment.ParentNode.RemoveChild(comment);
								}
							}

							// indexing
							indexing(doc);


							/*if (divNodes != null) {
								foreach (HtmlNode div in divNodes) {
									String content = div.InnerText;
									String[] words = content.Split(delimiterChars);
									using(var trans = sql.BeginTransaction()) {
										using(var cmd = sql.CreateCommand()) {
											foreach(String word in words) {
												if(word != "") {
													String addCommand = "insert into data (URL, word) values ('" + baseUrl + "', '" + word +"')";
													//Console.WriteLine(addCommand);
													cmd.CommandText = addCommand;
													//cmd.ExecuteNonQuery();
													swCrawl.WriteLine(word);
												}
											}
										}
										trans.Commit();
									}
											

									foreach (HtmlNode child in div.ChildNodes) {
										String childContent = child.InnerText;
										words = childContent.Split(delimiterChars);
										using(var trans = sql.BeginTransaction()) {
											using(var cmd = sql.CreateCommand()) {
												foreach (String word in words) {
													if(word != "") {
														swCrawl.WriteLine(word);
														String addCommand = "insert into data (URL, word) values ('"+ baseUrl + "', '" + word +"')";
														//Console.WriteLine(addCommand);
														cmd.CommandText = addCommand;
														//cmd.ExecuteNonQuery();
													}
												}
											}
											trans.Commit();
										}
									}
								}
							}*/
							
						}
					} catch (Exception e) {
						String s = e.ToString();
						Console.WriteLine(s);
					}
				}
				urlsToVisit = newUrl;
			}
		}

		private void crawlDFS(String baseUrl, int depth) {
			String urlpath = "";
			if (depth > DFS_MAX_DEPTH) {

			} else {
				urlsVisited.Add(baseUrl);

				try {
					String htmlText = getHtmlText(baseUrl);
					Console.WriteLine(baseUrl);
					swIndex.WriteLine("<LINK>");
					swIndex.WriteLine(baseUrl);
					swIndex.WriteLine("</>");
					if (htmlText != null) {
						HtmlDocument doc = new HtmlDocument();
						doc.LoadHtml(htmlText);

						// crawling
						foreach (HtmlNode attNode in doc.DocumentNode.SelectNodes("//a")) {
							String url = attNode.GetAttributeValue("href", null);
							if (url != null) {
								if (isUrlIgnored(url)) {
									continue;
								} else {
									url = new Uri(new Uri(baseUrl), url).AbsoluteUri.ToString();
								}
								if (!urlsVisited.Contains(url)) {
									Console.WriteLine(urlsVisited.Count);
									swCrawl.WriteLine(depth + " " + baseUrl + " " + url);
									if (depth <= DFS_MAX_DEPTH) {
										crawlDFS(url, depth + 1);
									}
								}
							}
						}
						HtmlNodeCollection divNodes = doc.DocumentNode.SelectNodes("//div");
						HtmlNodeCollection comments = doc.DocumentNode.SelectNodes("//comment()");
						
						if (comments != null) {
							foreach (HtmlNode comment in comments) {
								comment.ParentNode.RemoveChild(comment);
							}
						}

						// indexing
						indexing(doc);

						/*if (divNodes != null) {
							foreach (HtmlNode div in divNodes) {
								String content = div.InnerText;
								String[] words = content.Split(delimiterChars);
								using(var trans = sql.BeginTransaction()) {
									using(var cmd = sql.CreateCommand()) {
										foreach (String word in words) {
											if(word != "") {
												swCrawl.WriteLine(word);
												String addCommand = "insert into data (URL, word) values ('" + baseUrl + "', '" + word +"')";
												//Console.WriteLine(addCommand);
												cmd.CommandText = addCommand;
												//cmd.ExecuteNonQuery();
											}
										}
									}
									trans.Commit();
								}
								
								foreach (HtmlNode child in div.ChildNodes) {
									String childContent = child.InnerText;
									words = childContent.Split(delimiterChars);
									using(var trans = sql.BeginTransaction()) {
										using(var cmd = sql.CreateCommand()) {
											foreach (String word in words) {
												if(word != "") {
													swCrawl.WriteLine(word);
													String addCommand = "insert into data (URL, word) values ('" + baseUrl + "', '" + word +"')";
													//Console.WriteLine(addCommand);
													cmd.CommandText = addCommand;
													//cmd.ExecuteNonQuery();
												}	
											}
										}
										trans.Commit();
									}
								}
							}
						}*/
					}
				} catch (Exception e) {
					String s = e.ToString();
					System.Console.WriteLine(s);
				}
			}
			
		}

		private Boolean isUrlIgnored(String urlpath) {
			if (urlpath.Contains(".pdf") || urlpath.Contains(".png") || urlpath.Contains(".jpg")) {
				return true;
			}

			return false;
		}

		private void indexing(HtmlDocument doc) {
			swIndex.WriteLine("<TITLE>");
			foreach (HtmlNode titleNode in doc.DocumentNode.SelectNodes("//title")) {
				String[] words = titleNode.InnerText.Split(delimiterChars);
				foreach (String word in words) {
					if (!word.Equals("")) {
						swIndex.Write(word+ " ");
					}
				}
				swIndex.WriteLine();
			}
			swIndex.WriteLine("</>");

			/*swIndex.WriteLine("<LINK>");
			foreach (HtmlNode attNode in doc.DocumentNode.SelectNodes("//a")) {
				String[] words = attNode.InnerText.Split(delimiterChars);
				foreach (String word in words) {
					if (!word.Equals("")) {
						swIndex.WriteLine(word + " ");
					}
				}
			}
			swIndex.WriteLine("</LINK>");*/

			swIndex.WriteLine("<WORD>");
			foreach (HtmlNode div in doc.DocumentNode.SelectNodes("//div")) {
				String[] words = div.InnerText.Split(delimiterChars);
				foreach (String word in words) {
					if (!word.Equals("")) {
						swIndex.WriteLine(word);
					}
				}

				foreach (HtmlNode child in div.ChildNodes) {
					String childContent = child.InnerText;
					words = childContent.Split(delimiterChars);
					using(var trans = sql.BeginTransaction()) {
						using(var cmd = sql.CreateCommand()) {
							foreach (String word in words) {
								if(word != "") {
									swIndex.WriteLine(word);
									//String addCommand = "insert into data (URL, word) values ('" + baseUrl + "', '" + word +"')";
									//Console.WriteLine(addCommand);
									//cmd.CommandText = addCommand;
									//cmd.ExecuteNonQuery();
								}	
							}
						}
						trans.Commit();
					}
				}
			}
			swIndex.WriteLine("</>");
		}

		private void writeToDB() {
			const int LINK = 0;
			const int TITLE = 1;
			const int WORD = 2;

			int part = -1;
			string line, link = "", title = "", word = "";
			StreamReader file = new StreamReader("indexing.txt");
			using(var trans = sql.BeginTransaction()) {
				using(var cmd = sql.CreateCommand()) {	
					while((line = file.ReadLine()) != null) {
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
								String addCommand = "insert into data (URL, Title, Word) values ('" + link + "', '" + title +"', '"+ word + "')";
								//Console.WriteLine(addCommand);
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
						//System.Console.WriteLine("line " + line + " part " + part + " link "+ link + " title " + title + " word " + word);
					}
				}
				trans.Commit();
			}
		}

		private void search() {
			String input, query;
			input = System.Console.ReadLine();
			SQLiteDataReader reader;
			while(!input.Equals("exit")) {
				query = "SELECT URL, Title FROM data WHERE Word LIKE '%" + input + "%'";
				command.CommandText = query;
				reader = command.ExecuteReader();
				while(reader.Read()) {
					Console.WriteLine("URL = " + reader["URL"]);
					Console.WriteLine("Title = " + reader["Title"]);
				}
				input = Console.ReadLine();
			}
		}

	}

}
