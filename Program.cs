using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;

namespace bvre;

class Program {
	static async Task Main() {
		Boost booster = new();
		await booster.SetTarget(-1);
		await booster.GetProxies();
		await booster.TestProxies();
		await Task.Delay(2000);
		while (true) {
			await booster.RunProxies();
		}
	}
}
public class Boost {
	private static string[] ProxiesSources = [
		"https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/http.txt",
		"https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/socks4.txt",
		"https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/socks5.txt",
		"https://raw.githubusercontent.com/jetkai/proxy-list/main/online-proxies/txt/proxies.txt",
		"https://raw.githubusercontent.com/monosans/proxy-list/main/proxies/all.txt", //Linus Torvalds
		"https://raw.githubusercontent.com/roosterkid/openproxylist/main/HTTPS_RAW.txt",
		"https://raw.githubusercontent.com/almroot/proxylist/master/list.txt",
		"https://raw.githubusercontent.com/ShiftyTR/Proxy-List/master/proxy.txt",
		"https://raw.githubusercontent.com/hookzof/socks5_list/master/proxy.txt",
		"https://raw.githubusercontent.com/clarketm/proxy-list/master/proxy-list-raw.txt",
		"https://raw.githubusercontent.com/proxifly/free-proxy-list/main/proxies/all/data.txt",
		"https://raw.githubusercontent.com/ALIILAPRO/Proxy/main/http.txt",
		"https://raw.githubusercontent.com/ALIILAPRO/Proxy/main/socks4.txt",
		"https://raw.githubusercontent.com/ALIILAPRO/Proxy/main/socks5.txt",
		"https://raw.githubusercontent.com/Zaeem20/FREE_PROXIES_LIST/master/http.txt",
		"https://raw.githubusercontent.com/Zaeem20/FREE_PROXIES_LIST/master/https.txt",
		"https://raw.githubusercontent.com/Zaeem20/FREE_PROXIES_LIST/master/socks4.txt",
		"https://raw.githubusercontent.com/Zaeem20/FREE_PROXIES_LIST/master/socks5.txt",
		"https://raw.githubusercontent.com/vakhov/fresh-proxy-list/master/proxylist.txt",
		"https://raw.githubusercontent.com/r00tee/Proxy-List/main/Https.txt",
		"https://raw.githubusercontent.com/r00tee/Proxy-List/main/Socks4.txt",
		"https://raw.githubusercontent.com/r00tee/Proxy-List/main/Socks5.txt",
		"https://github.com/databay-labs/free-proxy-list/raw/master/http.txt",
		"https://github.com/databay-labs/free-proxy-list/raw/master/socks4.txt",
		"https://github.com/databay-labs/free-proxy-list/raw/master/socks5.txt",
		"https://github.com/elliottophellia/proxylist/raw/master/results/mix_checked.txt",
		"https://github.com/rdavydov/proxy-list/raw/main/proxies/http.txt",
		"https://github.com/rdavydov/proxy-list/raw/main/proxies/socks4.txt",
		"https://github.com/rdavydov/proxy-list/raw/main/proxies/socks5.txt",
		"https://github.com/prxchk/proxy-list/raw/main/all.txt",
		"https://github.com/iplocate/free-proxy-list/raw/refs/heads/main/all-proxies.txt",
		"https://api.proxyscrape.com/v2/?request=displayproxies&protocol=all&timeout=10000&country=all&simplified=true",
	];
	private static HttpClient UnproxiedClient = new();
	private static List<HttpClient>? ProxiedClients;
	private static List<string>? ProxiesToUse;
	private static StringContent? Payload;
	private static string Target = "https://api.scratch.mit.edu/users/thanh_cundz/projects/1334396955/views";
	private async Task<string[]> GetProxiesFromSource(string ProxiesSource) {
		try {
			string content = await UnproxiedClient.GetStringAsync(ProxiesSource);
			string[] proxies = content
				.Replace("http://", "")
				.Replace("socks4://", "")
				.Replace("socks5://", "")
				.Replace("https://", "")
				.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
			Console.WriteLine($"Fetched {proxies.Length} proxies from {ProxiesSource}");
			return proxies;
		} catch {
			Console.WriteLine($"Failed to fetch {ProxiesSource}");
			return new string[0];
		}
	}
	public async Task GetProxies() {
		int sourcesCount = ProxiesSources.Length;
		Task<string[]>[] workers = new Task<string[]>[sourcesCount+2];
		workers[0] = PM();
		workers[1] = GN();
		for (int i = 0; i < sourcesCount; i++) {
			workers[i+2] = GetProxiesFromSource(ProxiesSources[i]);
		}
		HashSet<string> result = new();
		foreach (string[] proxies in await Task.WhenAll(workers)) {
			foreach (string proxy in proxies)
				result.Add(proxy);
		}
		ProxiesToUse = result.ToList();
	}
	public async Task TestProxies() {
		ProxiedClients = new List<HttpClient>();
		HttpClientHandler handler;
		Payload = new StringContent("{}", Encoding.UTF8, "application/json");
		foreach (string proxy in ProxiesToUse!) {
			try {
				handler = new HttpClientHandler
				{
					Proxy = new WebProxy(proxy),
					UseProxy = true
				};
				HttpClient ProxiedClient = new HttpClient(handler);
				ProxiedClient.Timeout = TimeSpan.FromSeconds(10);
				ProxiedClients.Add(ProxiedClient);
			} catch {}
		}
		int clientsCount = ProxiedClients.Count;
		if (clientsCount == 0) throw new Exception("No proxy found!");
		HashSet<int> liveSet = new();
		for (int j = 0; j < 3; j++) {
			Task<int>[] workers = new Task<int>[clientsCount];
			for (int i = 0; i < clientsCount; i++) {
				workers[i] = TestProxyWorker(i);
			}
			foreach (int id in await Task.WhenAll(workers)) {
				if (id >= 0)
					liveSet.Add(id);
			}
		}
		for (int i = clientsCount-1; i >= 0; i--) {
			if (!liveSet.Contains(i)) {
				ProxiedClients?[i].Dispose();
				ProxiedClients?.RemoveAt(i);
			}
		}
		Console.WriteLine($"total {ProxiedClients?.Count} live proxies");
		ProxiesToUse = null;
	}
	private async Task<int> TestProxyWorker(int id) {
		HttpClient client = ProxiedClients?[id]!;
		try {
			var res = await client?.PostAsync(Target, Payload)!;
			int statusCode = (int)res.StatusCode;
			if (statusCode == 200 || statusCode == 429) {
				Console.WriteLine($"works: {id}");
			}
			return id;
		} catch {
			return -1;
		}
	}
	public async Task RunProxies() {
		int clientsCount = (int)ProxiedClients?.Count!;
		if (clientsCount == 0) throw new Exception("No live proxy found!");
		Task[] workers = new Task[clientsCount];
		for (int i = 0; i < clientsCount; i++) {
			workers[i] = RunProxyWorker(i);
		}
		await Task.WhenAll(workers);
	}
	private async Task RunProxyWorker(int id) {
		HttpClient client = ProxiedClients?[id]!;
		try {
			var res = await client?.PostAsync(Target, Payload)!;
			int statusCode = (int)res.StatusCode;
			Console.WriteLine($"received: {statusCode} {id}");
		} catch {}
	}
	public async Task SetTarget(int id) {
		int trueId = id;
		if (trueId == -1) {
			Console.Write("Enter project ID: ");
			if (!int.TryParse(Console.ReadLine(), out trueId)) {
				trueId = -1;
			}
		}
		if (trueId == -2) {
			Target = "https://api.scratch.mit.edu/users/thanh_cundz/projects/1334396955/views";
			return;
		}
		JsonDocument projectData = JsonDocument.Parse(
			await UnproxiedClient.GetStringAsync($"https://api.scratch.mit.edu/projects/{trueId}")
		);
		JsonElement author = projectData.RootElement.GetProperty("author");
		string projectAuthorUsername = author.GetProperty("username").GetString()!;
		Target = $"https://api.scratch.mit.edu/users/{projectAuthorUsername}/projects/{trueId}/views";
		Console.WriteLine(Target);
	}
	private async Task<string[]> PM() {
		try {
			string content = await UnproxiedClient.GetStringAsync("https://freeproxies-api.website.proxymaven.com/proxies?per_page=100000");
			JsonDocument jsonDoc = JsonDocument.Parse(content);
			JsonElement data = jsonDoc.RootElement.GetProperty("proxies");
			string[] proxies = data
				.EnumerateArray()
				.Select(e => e.GetProperty("proxy").GetString()!)
				.ToArray();
			Console.WriteLine($"Fetched {proxies.Length} proxies from PM");
			return proxies;
		} catch {
			Console.WriteLine("Failed to fetch PM");
			return new string[0];
		}
	}
	private async Task<string[]> GN() {
		try {
			string content = await UnproxiedClient.GetStringAsync("https://proxylist.geonode.com/api/proxy-list?limit=500");
			JsonDocument jsonDoc = JsonDocument.Parse(content);
			JsonElement data = jsonDoc.RootElement.GetProperty("data");
			string[] proxies = data
				.EnumerateArray()
				.Select(e => e.GetProperty("ip").GetString()! + ":" + e.GetProperty("port").GetString()!)
				.ToArray();
			Console.WriteLine($"Fetched {proxies.Length} proxies from GN");
			return proxies;
		} catch {
			Console.WriteLine("Failed to fetch GN");
			return new string[0];
		}
	}
}