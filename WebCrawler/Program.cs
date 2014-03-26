using System;
using System.Collections.Generic;

namespace WebCrawler {
	class Program {
		static void Main(string[] args) {
			
			Console.WriteLine("1. Crawling");
			//System.Console.WriteLine("2. Export data to MySQL");
			Console.WriteLine("Else. Begin Searching");
			string pil = Console.ReadLine();
			if (pil.Equals("1")) {
				System.Console.WriteLine("0. BFS 1. DFS");
				int crawltype = int.Parse(Console.ReadLine());
				Console.WriteLine(crawltype);
				Crawler crawler = new Crawler("http://www.itb.ac.id/", crawltype, 0);
				//Crawler crawler = new Crawler("http://informatika.stei.itb.ac.id/~rinaldi.munir/", crawltype, 0);
				//Crawler crawler = new Crawler("http://www.wikipedia.org", crawltype, 0);
				//Crawler crawler = new Crawler("http://www.facebook.com", crawltype, 0);
				//Crawler crawler = new Crawler("http://rinaldimunir.wordpress.com/", crawltype, 0);
			}

			Searcher searcher = new Searcher();
		}
	}
}
