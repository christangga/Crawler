using System;
using System.Collections.Generic;

namespace WebCrawler {
	class Program {
		static void Main(string[] args) {
			String link;
			Crawler crawler = new Crawler();
			Console.WriteLine("1. Crawling");
			Console.WriteLine("2. Export data to MySQL");
			Console.WriteLine("3. Begin Searching");
			string pil = Console.ReadLine();
			if (pil.Equals("1")) {
				Console.WriteLine("1. BFS");
				Console.WriteLine("2. DFS");
				int crawltype = (int.Parse(Console.ReadLine())) - 1;
				Console.WriteLine("1. Depth Limit");
				Console.WriteLine("2. Pages Limit");
				int limitType = int.Parse(Console.ReadLine());
				Console.WriteLine("Max Pages/Depth:");
				int limit = int.Parse(Console.ReadLine());
				if(limitType == 1)
					crawler.setMaxDepth(limit);
				else if(limitType == 2)
					crawler.setMaxPage(limit);
				Console.WriteLine("Enter link:");
				link = Console.ReadLine();
				crawler.crawl(link, crawltype, limitType);
				//Crawler crawler = new Crawler("http://informatika.stei.itb.ac.id/~rinaldi.munir/", crawltype, 0);
				//Crawler crawler = new Crawler("http://www.wikipedia.org", crawltype, 0);
				//Crawler crawler = new Crawler("http://www.facebook.com", crawltype, 0);
				//Crawler crawler = new Crawler("http://rinaldimunir.wordpress.com/", crawltype, 0);
			}
			if(pil.Equals("2")) {
				crawler.export();
			}
			if(pil.Equals("3")) {
				Searcher searcher = new Searcher();
			}
		}
	}
}
