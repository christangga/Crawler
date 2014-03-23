using HtmlAgilityPack;
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

		public Crawler(String baseUrl, int searchtype) {
			urlsToVisit = new LinkedList<String>();
			urlsVisited = new HashSet<String>();
			swCrawl = new StreamWriter("crawling.txt");
			swIndex = new StreamWriter("indexing.txt");

			Console.WriteLine("Initializing Crawler . . .");
			if (searchtype == 0) {
				crawlBFS(baseUrl);
			} else if (searchtype == 1) {
				crawlDFS(baseUrl, 0);
			}
			swCrawl.Close();
			Console.WriteLine("Done Crawling . . .");
		}

		private void addUrl(String urlpath) {
			urlsToVisit.AddLast(urlpath);
			urlsVisited.Add(urlpath);
		}

		private String getHtmlText(String urlpath) {
			try {
				HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create(urlpath);
				request.UserAgent = "Crawler";
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

							// indexing
							indexing(doc);

							/*
							HtmlNodeCollection divNodes = doc.DocumentNode.SelectNodes("//div");
							HtmlNodeCollection comments = doc.DocumentNode.SelectNodes("//comment()");

							if (comments != null) {
								foreach (HtmlNode comment in comments) {
									comment.ParentNode.RemoveChild(comment);
								}
							}

							if (divNodes != null) {
								foreach (HtmlNode div in divNodes) {
									String content = div.InnerText;
									String[] words = content.Split(delimiterChars);
									foreach (String word in words) {
										swCrawl.Write(word + " ");
									}

									foreach (HtmlNode child in div.ChildNodes) {
										String childContent = child.InnerText;
										words = childContent.Split(delimiterChars);
										foreach (String word in words) {
											swCrawl.Write(word + " ");
										}
									}
								}
							}
							*/
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
			if (depth > DFS_MAX_DEPTH) {

			} else {
				urlsVisited.Add(baseUrl);

				try {
					String htmlText = getHtmlText(baseUrl);
					Console.WriteLine(baseUrl);
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

						// indexing
						indexing(doc);

						/*
						HtmlNodeCollection divNodes = doc.DocumentNode.SelectNodes("//div");
						HtmlNodeCollection comments = doc.DocumentNode.SelectNodes("//comment()");
						 * 
						if (comments != null) {
							foreach (HtmlNode comment in comments) {
								comment.ParentNode.RemoveChild(comment);
							}
						}

						if (divNodes != null) {
							foreach (HtmlNode div in divNodes) {
								String content = div.InnerText;
								String[] words = content.Split(delimiterChars);
								foreach (String word in words) {
									swCrawl.Write(word);
								}

								foreach (HtmlNode child in div.ChildNodes) {
									String childContent = child.InnerText;
									words = childContent.Split(delimiterChars);
									foreach (String word in words) {
										swCrawl.Write(word);
									}

								}
							}
						}
						*/
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
			swIndex.WriteLine(">>>>> TITLE");
			foreach (HtmlNode titleNode in doc.DocumentNode.SelectNodes("//title")) {
				String[] words = titleNode.InnerText.Split(delimiterChars);
				foreach (String word in words) {
					if (!word.Equals("")) {
						swIndex.Write(word + " ");
					}
				}
				swIndex.WriteLine('\n');
			}
			swIndex.WriteLine("/TITLE <<<<<\n");

			swIndex.WriteLine(">>>>> ATTRIBUTE");
			foreach (HtmlNode attNode in doc.DocumentNode.SelectNodes("//a")) {
				String[] words = attNode.InnerText.Split(delimiterChars);
				foreach (String word in words) {
					if (!word.Equals("")) {
						swIndex.Write(word + " ");
					}
				}
				swIndex.WriteLine('\n');
			}
			swIndex.WriteLine("/ATTRIBUTE <<<<<\n");

			swIndex.WriteLine(">>>>> PARAGRAPH");
			foreach (HtmlNode pNode in doc.DocumentNode.SelectNodes("//p")) {
				String[] words = pNode.InnerText.Split(delimiterChars);
				foreach (String word in words) {
					if (!word.Equals("")) {
						swIndex.Write(word + " ");
					}
				}
				swIndex.WriteLine('\n');
			}
			swIndex.WriteLine("/PARAGRAPH <<<<<\n");
		}

	}

}
