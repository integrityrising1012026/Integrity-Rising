using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

class GumroadWorkbookIntegration
{
    private const string GUMROAD_API_KEY = "YOUR_GUMROAD_API_KEY";
    private const string ORDERS_FOLDER = "./orders";
    private const string WORKBOOKS_FOLDER = "./workbooks";

    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Integrity Rising Workbook Order System ===\n");
        
        // Create necessary folders
        CreateFolders();
        
        // Menu
        while (true)
        {
            Console.WriteLine("\nOptions:");
            Console.WriteLine("1. Fetch Gumroad Orders");
            Console.WriteLine("2. Save Order to File");
            Console.WriteLine("3. List Saved Orders");
            Console.WriteLine("4. Exit");
            Console.Write("\nSelect an option: ");
            
            string choice = Console.ReadLine();
            
            switch (choice)
            {
                case "1":
                    await FetchGumroadOrders();
                    break;
                case "2":
                    SaveOrderToFile();
                    break;
                case "3":
                    ListSavedOrders();
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("Invalid option. Try again.");
                    break;
            }
        }
    }

    static void CreateFolders()
    {
        if (!Directory.Exists(ORDERS_FOLDER))
            Directory.CreateDirectory(ORDERS_FOLDER);
        
        if (!Directory.Exists(WORKBOOKS_FOLDER))
            Directory.CreateDirectory(WORKBOOKS_FOLDER);
        
        Console.WriteLine("✓ Folders created/verified");
    }

    static async Task FetchGumroadOrders()
    {
        try
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {GUMROAD_API_KEY}");
                
                // Replace with your Gumroad product ID
                string gumroadUrl = "https://api.gumroad.com/v2/products";
                var response = await client.GetAsync(gumroadUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("\n✓ Orders fetched successfully!");
                    Console.WriteLine($"Response: {content}");
                }
                else
                {
                    Console.WriteLine($"\n✗ Error: {response.StatusCode}");
                    Console.WriteLine("Make sure your Gumroad API key is set correctly.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error fetching orders: {ex.Message}");
        }
    }

    static void SaveOrderToFile()
    {
        Console.Write("\nEnter customer name: ");
        string customerName = Console.ReadLine();
        
        Console.Write("Enter workbook title: ");
        string workbookTitle = Console.ReadLine();
        
        Console.Write("Enter order date (yyyy-MM-dd): ");
        string orderDate = Console.ReadLine();
        
        string orderContent = $@"=== WORKBOOK ORDER ===
Customer: {customerName}
Workbook: {workbookTitle}
Date: {orderDate}
Status: Completed
Downloaded: Yes
";
        
        string fileName = $"{ORDERS_FOLDER}/{customerName}_{workbookTitle}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        
        try
        {
            File.WriteAllText(fileName, orderContent);
            Console.WriteLine($"\n✓ Order saved to: {fileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error saving order: {ex.Message}");
        }
    }

    static void ListSavedOrders()
    {
        try
        {
            string[] files = Directory.GetFiles(ORDERS_FOLDER);
            
            if (files.Length == 0)
            {
                Console.WriteLine("\nNo orders saved yet.");
                return;
            }
            
            Console.WriteLine("\n=== SAVED ORDERS ===");
            foreach (string file in files)
            {
                string content = File.ReadAllText(file);
                Console.WriteLine($"\n{Path.GetFileName(file)}:");
                Console.WriteLine(content);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error listing orders: {ex.Message}");
        }
    }
}
