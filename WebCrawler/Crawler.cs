﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using MySql.Data.MySqlClient;

namespace WebCrawler {

	class Crawler {
		private const int BFS_MAX_DEPTH = 2;
		private const int DFS_MAX_DEPTH = 2;
		private const int BFS_MAX_PAGES = 1000;
		private const int DFS_MAX_PAGES = 1000;

		public static string[] delimiterStrings = { " ", ",", ".", ":", "\t", "\n", "?", "\\", "/", "!", "@", "\r", "(", ")", "\"", "\'", "<", ">", "[", "]", "{", "}", "|", "”", "“", "~", "#", "&", "-" };

		private LinkedList<String> urlsToVisit = new LinkedList<String>();
		private HashSet<String> urlsVisitHash = new HashSet<String>();
		private HashSet<String> urlsVisited = new HashSet<String>();
		private StreamWriter swCrawl = new StreamWriter("crawling2.txt");
		private StreamWriter swIndex = new StreamWriter("indexing2.txt");
		private StreamWriter swErrLog = new StreamWriter("errlog2.txt");
		private StreamWriter exportLine = new StreamWriter("exportline2.txt");
		private MySqlConnection sql = new MySqlConnection(@"server=localhost;database=SQL;userid=root;");
		private MySqlCommand command;
		private int lineNumbers;

		public Crawler(String baseUrl, int crawltype, int maxpages) {
			lineNumbers = 0;
			//String createDB = "create table if not exists data (URL varchar(200), Title varchar(200), Word varchar(100), UNIQUE (URL, Title, Word) on conflict replace)";
			//command.CommandText = createDB;
			//command.ExecuteNonQuery();
			//SQLiteConnection.CreateFile("SQL.sqlite");
			sql.Open();
			command = sql.CreateCommand();

			Console.WriteLine("Initializing Crawler . . .");
			/*if (crawltype == 0) {
				//crawlBFSdepth(baseUrl);
				crawlBFSpages(baseUrl);
			} else if (crawltype == 1) {
				//crawlDFSdepth(baseUrl, 0);
				crawlDFSpages(baseUrl, 0);
			}*/
			swCrawl.Close();
			swIndex.Close();
			swErrLog.Close();
			exportLine.WriteLine(lineNumbers);
			exportLine.Close();
			StreamReader file = new StreamReader("exportline.txt");
			lineNumbers = 295000;//int.Parse(file.ReadLine());
			file.Close();
			Console.WriteLine("Done Crawling . . .");
			Console.WriteLine("Exporting to MySQL . . .");
			writeToDB();
			Console.WriteLine("Exporting to MySQL done . . .");
			sql.Close();
		}

		private void addUrl(String urlpath) {
			urlsToVisit.AddLast(urlpath);
			urlsVisitHash.Add(urlpath);
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
				swErrLog.WriteLine(s);
			}
			return null;
		}

		private void crawlBFSdepth(String baseUrl) {
			swCrawl.WriteLine(">>>>>>>>>> BFS LEVEL 0 <<<<<<<<<<\n");
			addUrl(baseUrl);
			swCrawl.WriteLine(urlsVisited.Count + " " + baseUrl + "\n");
			for (int i = 1; i <= BFS_MAX_DEPTH; ++i) {
				LinkedList<String> newUrl = new LinkedList<String>();

				swCrawl.WriteLine(">>>>>>>>>> BFS LEVEL " + i + " <<<<<<<<<<\n");
				foreach (String urlpath in urlsToVisit) {
					try {
						String htmlText = getHtmlText(urlpath);
						swIndex.WriteLine("<LINK>"); lineNumbers++;
						swIndex.WriteLine(urlpath); lineNumbers++;
						swIndex.WriteLine("</>"); lineNumbers++;
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
						}
					} catch (Exception e) {
						String s = e.ToString();
						swErrLog.WriteLine(s);
					}
				}
				urlsToVisit = newUrl;
			}
		}

		private void crawlBFSpages(String baseUrl) {
			int counter = 0;
			addUrl(baseUrl);
			++counter;
			swCrawl.WriteLine(urlsVisited.Count + " " + baseUrl + "\n");
			while(urlsVisited.Count < BFS_MAX_PAGES) {
				LinkedListNode<String> node = urlsToVisit.First;
				String urlpath = node.Value;
				urlsToVisit.RemoveFirst();
				Console.WriteLine("Connecting to " + urlpath);
				try {
					String htmlText = getHtmlText(urlpath);
					swIndex.WriteLine("<LINK>"); lineNumbers++;
					swIndex.WriteLine(urlpath); lineNumbers++;
					swIndex.WriteLine("</>"); lineNumbers++;
					if (htmlText != null) {
						HtmlDocument doc = new HtmlDocument();
						doc.LoadHtml(htmlText);

						// crawling
						foreach (HtmlNode attNode in doc.DocumentNode.SelectNodes("//a")) {
							String url = attNode.GetAttributeValue("href", null);
							if (url != null) {
								url = new Uri(new Uri(urlpath), url).AbsoluteUri.ToString();
								if (!urlsVisitHash.Contains(url) && !isUrlIgnored(url)) {
									addUrl(url);
									swCrawl.WriteLine(urlsVisited.Count + " " + urlpath + " " + url);
								}
								Console.WriteLine(urlsVisited.Count + " " + urlsToVisit.Count + " " + url);
							}
						}
						Console.WriteLine(urlsVisited.Count + " " + urlsToVisit.Count + " ");
						HtmlNodeCollection divNodes = doc.DocumentNode.SelectNodes("//div");
						HtmlNodeCollection comments = doc.DocumentNode.SelectNodes("//comment()");

						if (comments != null) {
							foreach (HtmlNode comment in comments) {
								comment.ParentNode.RemoveChild(comment);
							}
						}

						// indexing
						indexing(doc);
						urlsVisited.Add(urlpath);
					}
				} catch (Exception e) {
					String s = e.ToString();
					swErrLog.WriteLine(s);
				}
			}
		}

		private void crawlDFSdepth(String baseUrl, int depth) {
			if (depth > DFS_MAX_DEPTH) {

			} else {
				urlsVisited.Add(baseUrl);

				try {
					String htmlText = getHtmlText(baseUrl);
					//Console.WriteLine(baseUrl);
					swIndex.WriteLine("<LINK>"); lineNumbers++;
					swIndex.WriteLine(baseUrl); lineNumbers++;
					swIndex.WriteLine("</>"); lineNumbers++;
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
										crawlDFSdepth(url, depth + 1);
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
					}
				} catch (Exception e) {
					String s = e.ToString();
					swErrLog.WriteLine(s);
				}
			}
			
		}

		private void crawlDFSpages(String baseUrl, int counter) {
			if (counter > DFS_MAX_PAGES) {

			} else {
				urlsVisited.Add(baseUrl);

				try {
					String htmlText = getHtmlText(baseUrl);
					//Console.WriteLine(baseUrl);
					swIndex.WriteLine("<LINK>"); lineNumbers++;
					swIndex.WriteLine(baseUrl); lineNumbers++;
					swIndex.WriteLine("</>"); lineNumbers++;
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
									swCrawl.WriteLine(counter + " " + baseUrl + " " + url);
									if (counter <= DFS_MAX_PAGES) {
										crawlDFSdepth(url, counter + 1);
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
					}
				} catch (Exception e) {
					String s = e.ToString();
					swErrLog.WriteLine(s);
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
			int ndiv = 0, nword = 0;
			Console.WriteLine("indexing title");
			swIndex.WriteLine("<TITLE>"); lineNumbers++;
			foreach (HtmlNode titleNode in doc.DocumentNode.SelectNodes("//title")) {
				String[] words = titleNode.InnerText.Split(delimiterStrings, System.StringSplitOptions.RemoveEmptyEntries);
				foreach (String word in words) {
					if (!word.Equals("")) {
						swIndex.Write(word+ " "); lineNumbers++;
					}
				}
				swIndex.WriteLine(); lineNumbers++;
			}
			swIndex.WriteLine("</>"); lineNumbers++;
			swIndex.WriteLine("<WORD>"); lineNumbers++;
			Console.WriteLine("indexing div");
			foreach (HtmlNode div in doc.DocumentNode.SelectNodes("//div")) {
				int nchild = 0;
				ndiv++;
				//Console.WriteLine("ndiv " + ndiv);
				String[] words = div.InnerText.Split(delimiterStrings, StringSplitOptions.RemoveEmptyEntries);
				foreach (String word in words) {
					nword++;
					//Console.WriteLine("nword " + nword);
					if (!word.Equals("")) {
						swIndex.Write(word + " "); lineNumbers++;
					}
				}
				swIndex.WriteLine(); lineNumbers++;

				foreach (HtmlNode child in div.ChildNodes) {
					nchild++;
					//Console.WriteLine("nchild " + nchild);
					String childContent = child.InnerText;
					words = childContent.Split(delimiterStrings, StringSplitOptions.RemoveEmptyEntries);
					foreach (String word in words) {
						nword++;
						//Console.WriteLine("nword " + nword);
						if(!word.Equals("")) {
							swIndex.Write(word + " "); lineNumbers++;
						}	
					}
					swIndex.WriteLine(); lineNumbers++;
				}
			}
			Console.WriteLine("done indexing div");
			swIndex.WriteLine("</>"); lineNumbers++;
		}

		private void writeToDB() {
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
						Console.Clear();
						Console.WriteLine("Exporting to MySQL " + Iline + " " + lineNumbers);
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
								if(!word.Equals("")) {
									String addCommand = "replace into data (URL, Title, Word) values ('" + link + "', '" + title +"', '"+ word + "')";
									//Console.WriteLine(addCommand);
									cmd.CommandText = addCommand;
									cmd.ExecuteNonQuery();	
								}
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
	}

}
