using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using MySql.Data.MySqlClient;

namespace WebCrawler {

	class Crawler {
		private int BFS_MAX_DEPTH;
		private int DFS_MAX_DEPTH;
		private int BFS_MAX_PAGES;
		private int DFS_MAX_PAGES;

		public static string[] delimiterStrings = { " ", ",", ".", ":", "\t", "\n", "?", "\\", "/", "!", "@", "\r", "(", ")", "\"", "\'", "<", ">", "[", "]", "{", "}", "|", "”", "“", "~", "#", "&", "-", ";" };

		private LinkedList<String> urlsToVisit;
		private HashSet<String> urlsVisitHash;
		private HashSet<String> urlsVisited;
		private StreamWriter swCrawl;
		private StreamWriter swIndex;
		private StreamWriter swErrLog;
		private StreamWriter exportLine;
		private WebProxy proxy;
		private int lineNumbers;

		public Crawler() {
			urlsToVisit = new LinkedList<String>();
			urlsVisitHash = new HashSet<String>();
			urlsVisited = new HashSet<String>();
			swCrawl = new StreamWriter("crawling.txt");
			swIndex = new StreamWriter("indexing.txt");
			swErrLog = new StreamWriter("errlog.txt");
			exportLine = new StreamWriter("exportline.txt");
			proxy = new WebProxy("http://cache.itb.ac.id:8080", true);
			proxy.Credentials = new NetworkCredential("gilangjulians", "wakwawwawaww12112");
		}

		public void crawl(String baseUrl, int crawltype, int limitType) {
			lineNumbers = 0;
			Console.WriteLine("Initializing Crawler . . .");
			if (crawltype == 0) {
				if(limitType == 1)
					crawlBFSdepth(baseUrl);
				else if(limitType == 2)
					crawlBFSpages(baseUrl);
			} else if (crawltype == 1) {
				if(limitType == 1)
					crawlDFSdepth(baseUrl, 0);
				else if(limitType == 2)
					crawlDFSpages(baseUrl, 0);
			}
			exportLine.WriteLine(lineNumbers);

			swCrawl.Close();
			swIndex.Close();
			swErrLog.Close();
			exportLine.Close();
		}

		private void addUrl(String urlpath) {
			urlsToVisit.AddLast(urlpath);
			urlsVisitHash.Add(urlpath);
		}

		private String getHtmlText(String urlpath) {
			try {
				HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create(urlpath);
				request.UserAgent = "Crawler";
				request.Proxy = proxy;
				//request.Timeout = 1000;
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
									url = new Uri(new Uri(urlpath), url).AbsoluteUri.ToString();
									if (!urlsVisitHash.Contains(url) && !isUrlIgnored(url)) {
										newUrl.AddLast(url);
										urlsVisitHash.Add(url);
										Console.WriteLine(urlsVisited.Count);
										swCrawl.WriteLine(urlsVisited.Count + " " + url);
									}
									Console.WriteLine(urlsVisited.Count + " " + urlsToVisit.Count + " " + url);
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
							urlsVisited.Add(urlpath);
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
			while(urlsVisited.Count < BFS_MAX_PAGES && urlsToVisit.Count > 0) {
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
									counter++;
									addUrl(url);
									swCrawl.WriteLine(counter + " " + urlpath + " " + url);
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
					Console.WriteLine(s);
				}
			}
		}

		private void crawlDFSdepth(String baseUrl, int depth) {
			if (depth < DFS_MAX_DEPTH) {
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
								url = new Uri(new Uri(baseUrl), url).AbsoluteUri.ToString();
								if (!urlsVisitHash.Contains(url) && !isUrlIgnored(url)) {
									urlsVisitHash.Add(url);
									swCrawl.WriteLine(depth + " " + baseUrl + " " + url);
									if (depth <= DFS_MAX_DEPTH) {
										crawlDFSdepth(url, depth + 1);
									}
								}
							}
						}
						Console.WriteLine(urlsVisited.Count + " " + baseUrl);
						HtmlNodeCollection divNodes = doc.DocumentNode.SelectNodes("//div");
						HtmlNodeCollection comments = doc.DocumentNode.SelectNodes("//comment()");
						
						if (comments != null) {
							foreach (HtmlNode comment in comments) {
								comment.ParentNode.RemoveChild(comment);
							}
						}

						// indexing
						indexing(doc);
						urlsVisited.Add("dummy");
					}
				} catch (Exception e) {
					String s = e.ToString();
					swErrLog.WriteLine(s);
				}
			}
			
		}

		private void crawlDFSpages(String baseUrl, int counter) {
			if (urlsVisited.Count < DFS_MAX_PAGES) {
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
								url = new Uri(new Uri(baseUrl), url).AbsoluteUri.ToString();
								if (!urlsVisitHash.Contains(url)) {
									counter++;
									urlsVisitHash.Add(url);
									swCrawl.WriteLine(counter + " " + baseUrl + " " + url);
									if (counter <= DFS_MAX_PAGES) {
										crawlDFSdepth(url, counter + 1);
									}
								}
							}
						}
						Console.WriteLine(urlsVisited.Count + " " + baseUrl);
						HtmlNodeCollection divNodes = doc.DocumentNode.SelectNodes("//div");
						HtmlNodeCollection comments = doc.DocumentNode.SelectNodes("//comment()");

						if (comments != null) {
							foreach (HtmlNode comment in comments) {
								comment.ParentNode.RemoveChild(comment);
							}
						}

						// indexing
						indexing(doc);
						urlsVisited.Add("dummy");
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
			HashSet<String> entry = new HashSet<String>();
			foreach (HtmlNode div in doc.DocumentNode.SelectNodes("//div")) {
				int nchild = 0;
				ndiv++;
				String[] words = div.InnerText.Split(delimiterStrings, StringSplitOptions.RemoveEmptyEntries);
				foreach (String word in words) {
					nword++;
					if (!word.Equals("")) {
						if(!entry.Contains(word)) {
							entry.Add(word);
						}
					}
				}

				foreach (HtmlNode child in div.ChildNodes) {
					nchild++;
					String childContent = child.InnerText;
					words = childContent.Split(delimiterStrings, StringSplitOptions.RemoveEmptyEntries);
					foreach (String word in words) {
						nword++;
						if(!word.Equals("")) {
							if(!entry.Contains(word)) {
								entry.Add(word);
							}
						}	
					}
				}
			}
			foreach(String word in entry) {
				swIndex.WriteLine(word); lineNumbers++;
			}
			Console.WriteLine("done indexing div");
			swIndex.WriteLine("</>"); lineNumbers++;
		}

		public void setMaxDepth(int n) {
			DFS_MAX_DEPTH = n;
			BFS_MAX_DEPTH = n;
		}

		public void setMaxPage(int n) {
			DFS_MAX_PAGES = n;
			BFS_MAX_PAGES = n;
		}
	}

}
