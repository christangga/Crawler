﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace WebCrawler {

    class Crawler {
		private const int BFS_MAX_DEPTH = 2;
		private const int DFS_MAX_DEPTH = 1000;

		private static LinkedList<String> urlsToVisit;
		private static HashSet<String> urlsVisited;
		private static StreamWriter sw;
		private static String baseUrl;

		public Crawler(String urlpath, int searchtype) {
			urlsToVisit = new LinkedList<String>();
			urlsVisited = new HashSet<String>();
			sw = new StreamWriter("out.txt");
			baseUrl = urlpath;

			addUrl(urlpath);
			sw.WriteLine(urlsVisited.Count() + " " + urlsToVisit.Count() + " " + urlpath + '\n');
			if (searchtype == 0) {
				crawlBFS();
			} else if (searchtype == 1) {
				crawlDFS();
			}
			sw.Close();
		}

		private static void addUrl(String urlpath) {
			urlsToVisit.AddLast(urlpath);
			urlsVisited.Add(urlpath);
		}

		private static bool queueEmpty() {
			return urlsToVisit.Count == 0;
		}

		private static void crawlBFS() {
			for (int i = 1; i < BFS_MAX_DEPTH; ++i) {
				sw.WriteLine("BFS LEVEL " + i + '\n');
				LinkedList<String> newUrl = new LinkedList<String>();

				foreach (String urlpath in urlsToVisit) {
					String htmlText = getHtmlText(urlpath);
					try {
						HtmlDocument doc = new HtmlDocument();
						doc.LoadHtml(htmlText);
						HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//a[@href]");
						foreach (HtmlNode node in nodes) {
							String url = node.Attributes[0].Value;
							if (url.Contains(".pdf")) {
								continue;
							} else if (!url.Contains("http")) {
								if (baseUrl[baseUrl.Length - 1] == '/' && url[0] == '/') {
									url = baseUrl.Substring(0, baseUrl.Length - 1) + url;
								} else if (baseUrl[baseUrl.Length - 1] == '/' || url[0] == '/') {
									url = baseUrl + url;
								} else {
									url = baseUrl + "/" + url;
								}
							}
							if (url[url.Length - 1] != '/') {
								url += '/';
							}
							if (!urlsVisited.Contains(url)) {
								newUrl.AddLast(url);
								urlsVisited.Add(url);
								sw.WriteLine(urlsVisited.Count() + " " + newUrl.Count() + " " + urlpath + '\n' + url + '\n');
							} else {
								sw.WriteLine(">>>>> DUPLIKASI " + urlsVisited.Count() + " " + newUrl.Count() + " " + urlpath + '\n' + url + '\n');
							}

							/*Regex regex = new Regex("<a[^>]+href\\s*=\\s*['\"]([^'\"]+)['\"][^>]*>");
							//Regex regex = new Regex(@"<" + "a" + @"[^>]*?HREF\s*=\s*[""']?([^'"" >]+?)[ '""]?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
							//Regex regex = new Regex("\\s*(?i)href\\s*=\\s*(\"([^\"]*\")|'[^']*'|([^'\">\\s]+))");
					
							MatchCollection matches = regex.Matches(htmlText);
							//Console.WriteLine(matches.Count);

							foreach (Match match in matches) {
								//Regex avalue = new Regex("(\\S+)=[\"\']?((?:.(?![\"\']?\\s+(?:\\S+)=|[>\"\']))+.)[\"\']?");
								//Match m = avalue.Match(match.Value);
								String url = match.Value;
								Console.WriteLine(url);

								Regex value = new Regex("<a[^>]+href\\s*=\\s*['\"]([^'\"]+)['\"][^>]*>");
							
								if (url.Contains("http")) {
									Console.WriteLine("masuk 1");
									url = url.Substring(url.IndexOf("href") + 6, url.LastIndexOf(">") - (url.IndexOf("href") + 7));
								} else if (url.Contains("://")) {
									Console.WriteLine("masuk 2");
									url = url.Substring(url.IndexOf("href") + 6, url.LastIndexOf(">") - (url.IndexOf("href") + 7));
								} else {
									Console.WriteLine("masuk 3");
									url = url.Substring(url.IndexOf("href") + 6, url.LastIndexOf(">") - (url.IndexOf("href") + 7));
									if (url[0] != '/' && urlpath[urlpath.Length - 1] != '/') {
										url = urlpath + "/" + url;
									} else {
										url = urlpath + url;
									}
								}
								Regex ignore = new Regex("([^\\s]+(\\.(?i)(jpg|png|gif|bmp|doc|docx|ppt|pptx|pdf|wmv))$)");*/
						}
					} catch (Exception e) {
					
					}
				}
				urlsToVisit = newUrl;
			}
		}

		private static void crawlDFS() {
			for (int i = 0; i < DFS_MAX_DEPTH; ++i) {
				//String htmlText = getHtmlText(urlpath);
			
			}

		}

		private static String getHtmlText(String urlpath) {
			try {
				HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create(urlpath);
				request.UserAgent = "Web Crawler";
				WebResponse response = request.GetResponse();
				Stream stream = response.GetResponseStream();
				StreamReader reader = new StreamReader(stream);
				String htmlText = reader.ReadToEnd();
				//Console.WriteLine(htmlText);
				return htmlText;
			} catch (Exception e) {
			
			}
			return null;
		}

    }

}
