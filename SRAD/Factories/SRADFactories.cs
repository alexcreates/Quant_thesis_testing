using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Models;
using MySql.Data.MySqlClient;
using static Models.User;

namespace Factories
{ 
    public class OpenVolumeFactories
    {
        static string server = "localhost";
        static string db = "sakila_db"; //Change to your schema name
        static string port = "3306"; //Potentially 8889
        static string user = "root";
        static string pass = "root";
        internal static IDbConnection Connection {
            get {
                return new MySqlConnection($"Server={server};Port={port};Database={db};UserID={user};Password={pass};SslMode=None");
            }
        }


////////////////////////////////////////////////////////
////////////////////////////////////////////////////////
////////////////////////////////////////////////////////
//              USER LOGIN / REGISTRATION 


       
    public User GetUserById(int sessionId)
    {
        using (IDbConnection dbConnection = Connection)
        {
            dbConnection.Open();
            return dbConnection.Query<User>($"SELECT * FROM Users WHERE idUsers = '{sessionId}';").SingleOrDefault();
        }
    }


    public User GetUserByEmail(string email)
    {
        using (IDbConnection dbConnection = Connection)
        {
            dbConnection.Open();
            return dbConnection.Query<User>($"SELECT * FROM Users WHERE Email = '{email}'").SingleOrDefault();
        }
    }


    public bool EmailCheck(string email)
    {
        using (IDbConnection dbConnection = Connection)
        {
            dbConnection.Open();
            User userResult = dbConnection.Query<User>($"SELECT * FROM Users WHERE Email = '{email}'").LastOrDefault();
            if(userResult == null)
                return false;
            return true;
        }
    }


    public bool PasswordCheck(LogUser loguser)
    {
        using (IDbConnection dbConnection = Connection)
        {
            dbConnection.Open();
            PasswordHasher<LogUser> hasher = new PasswordHasher<LogUser>();  
            string storedPW = dbConnection.Query<string>($"SELECT Password FROM Users WHERE Email = '{loguser.Email}';").FirstOrDefault();              
            PasswordVerificationResult pwResult = hasher.VerifyHashedPassword(loguser, storedPW, loguser.Password);
            if(pwResult == 0)
                return false;
            return true;

        }
    }


    public void CreateUser(User user)
    {
        using (IDbConnection dbConnection = Connection)
        {
            dbConnection.Open();
            PasswordHasher<User> hasher = new PasswordHasher<User>();
            string h_pw = hasher.HashPassword(user, user.Password);
            dbConnection.Execute($"INSERT INTO Users (FirstName, LastName, Email, Password, createdAt, updatedAt) VALUES ('{user.FirstName}', '{user.LastName}', '{user.Email}', '{h_pw}', NOW(), NOW());");
        }
    }


    public int GetIDFromEmail(string email)
    {
        using (IDbConnection dbConnection = Connection)
        {
            dbConnection.Open();
            return dbConnection.Query<int>($"SELECT idUsers FROM Users WHERE Email = '{email}';").SingleOrDefault();
        }
    }



////////////////////////////////////////////////////////
////////////////////////////////////////////////////////
////////////////////////////////////////////////////////
//              STOCK METHODS


    public bool StockExists(string symbol)
    {
        using (IDbConnection dbConnection = Connection)
        {
            dbConnection.Open();
            Stock stockToCheck = dbConnection.Query<Stock>($"SELECT * FROM Stocks WHERE Symbol = '{symbol}'").FirstOrDefault();
            if(stockToCheck == null)
                return false;
            return true;
        }
    }


    public void CreateRecord(Stock stock)
    {
        using (IDbConnection dbConnection = Connection)
        {
            dbConnection.Open();
            double initOarResult = (stock.ResPoint - stock.SupPoint);
            dbConnection.Execute($@"INSERT INTO Stocks (Symbol, Last, CompName, ResPoint, SupPoint, CurrentOar, RelativeVolume, Comment, createdAt, updatedAt) 
                                    VALUES ('{stock.Symbol}', '{stock.Last}', '{stock.CompName}', '{stock.ResPoint}', '{stock.SupPoint}', '{initOarResult}', 0, '{stock.Comment}', NOW(), NOW());");
            int newStockId = dbConnection.Query<int>($"SELECT idStocks FROM Stocks WHERE Symbol = '{stock.Symbol}'").SingleOrDefault();
            dbConnection.Execute($@"INSERT INTO Oars (idStocks, SupPoint, ResPoint, OarResult, createdAt, updatedAt) 
                                    VALUES ('{newStockId}', '{stock.SupPoint}', '{stock.ResPoint}', '{initOarResult}', NOW(), NOW());");
        }
    }

    public Stock GetStockById(int idstocks)
    {
        using(IDbConnection dbConnection = Connection)
        {
            return dbConnection.Query<Stock>($"SELECT * FROM Stocks WHERE idstocks = '{idstocks}'").SingleOrDefault();
        }
    }


    public void UpdateResAndSupPoints(string symbol, double SupPoint, double ResPoint)
    {
        using(IDbConnection dbConnection = Connection)
        {
            // ResPoint = High of oscillation
            // SupPoint = Low of oscillation
            double OARResult = (ResPoint - SupPoint);
            Stock stock = GetStockBySymbol(symbol);
            dbConnection.Open();
            List<double> OarList = dbConnection.Query<double>($"SELECT OarResult FROM Oars WHERE idStocks = '{stock.idStocks}'").ToList();
            OarList.Add(OARResult);

            double Sum = 0;

            for(var i = 0; i < OarList.Count(); i++){
                Sum += OarList[i];
            }

            double NewAverageOar = Sum / OarList.Count();

            dbConnection.Execute($@"UPDATE Stocks SET CurrentOar = '{NewAverageOar}' WHERE idStocks = '{stock.idStocks}';");
            dbConnection.Execute($@"INSERT INTO Oars (idStocks, SupPoint, ResPoint, OarResult) VALUES ('{stock.idStocks}', '{SupPoint}', '{ResPoint}', '{OARResult}')");
        }
    }


    public Stock GetOARResultBySymbol(string symbol)
    {
        using(IDbConnection dbConnection = Connection)
        {
            dbConnection.Open();
            return dbConnection.Query<Stock>($"SELECT * FROM Stocks WHERE Symbol = '{symbol}'").LastOrDefault();
        }
    }


    public float GetMaxGainPercentage(int ResPoint, int SupPoint)
    {
        // The evaluation of ResPoint - SupPoint calculates the difference/increase
        return ((ResPoint - SupPoint) / SupPoint) * 100;
    }
    

    public float GetDecreasePercentage(int ResPoint, int SupPoint)
    {
        return ((ResPoint - SupPoint) / ResPoint) * 100; 
    }


    public void CreateOpenVolTable(int providedRange)
    {
        double StartVolume = 10000;
        using(IDbConnection dbConnection = Connection)
        {
            dbConnection.Open();         
            for(var i = 0; i < 100; i++){
                double openVolResult = (StartVolume / providedRange);
                dbConnection.Execute($@"INSERT INTO OpenVolume (RelativeFloat, Volume, Result) 
                                        VALUES ('{providedRange}', '{StartVolume}', '{openVolResult}');");
                StartVolume += 10000;
            }
        }
    }
        
    

    public List<OpenVolumeobj> GetRecordsByFloatRequest(int floatRequested)
    {
        using(IDbConnection dbConnection = Connection)
        {
            dbConnection.Open();
            return dbConnection.Query<OpenVolumeobj>($"SELECT * FROM OpenVolume WHERE Float = '{floatRequested}'").ToList();
        }
    }


    public Stock GetStockBySymbol(string symbol)
    {
        using(IDbConnection dbConnection = Connection)
        {
            dbConnection.Open();
            return dbConnection.Query<Stock>($"SELECT * FROM Stocks WHERE Symbol = '{symbol}'").SingleOrDefault();
        }
    }


    public Chart calcTargetChart(double stockPrice, int userCost)
    {
        using(IDbConnection dbConnection = Connection)
        {
            Chart freshChart = new Chart();

            // Create the List of Stock Prices
            List<double> stockPriceList = new List<double>();
            for(var i = 0; i < 9; i++){
                stockPriceList.Add(stockPrice);
                stockPrice += 0.10;
            }

            // Create The List of Share Sizes
            List<double> shareSizeList = new List<double>();
            for(var i = 0; i < 9; i++){
                shareSizeList.Add(userCost / stockPrice);
                stockPrice += 0.10;
            }

            // Create the Target List (relevant to the dollar amount of the stock price)
            // This takes the current stock price at each increment of +0.10 and multiplies the target for the gain by 0.10
            List<double> targetList = new List<double>();
            foreach(var val in stockPriceList){
                targetList.Add(val * 0.10);
            }
            
            // Create the Winnings List
            List<double> winningsList = new List<double>();
            int step = 0;
            foreach(var shareSize in shareSizeList){
                winningsList.Add(shareSize * targetList[step]);
                step++;
            }
            
            freshChart.stockPriceList = stockPriceList;
            freshChart.shareSizeList = shareSizeList;
            freshChart.targetList = targetList;
            freshChart.winningsList = winningsList;

            dbConnection.Open();
            // To keep the database scalable we are creating records of the first increment
            // and then calculating the results from a query with search by's 
            // to be determined
            dbConnection.Execute($@"INSERT INTO TargetCharts (stockPrice, userCost, shareSize, target, winnings, createdAt)
                                    VALUES ('{stockPrice}', '{userCost}', '{shareSizeList.Last()}', '{targetList.Last()}', '{winningsList.Last()}', NOW());");
            return freshChart;
        }
    }



    

    



   //////////////////////////////
            // GET ALLS  //


    public List<Oar> GetAllSupPointsById(int idStocks)
    {
        using(IDbConnection dbConnection = Connection)
        {
            dbConnection.Open();
            return dbConnection.Query<Oar>($"SELECT SupPoint FROM Oars WHERE idStocks = '{idStocks}'").ToList();
        }
    }



    public List<Oar> GetAllResPointsById(int idStocks)
    {
        using(IDbConnection dbConnection = Connection)
        {
            dbConnection.Open();
            return dbConnection.Query<Oar>($"SELECT ResPoint FROM Oars WHERE idStocks = '{idStocks}'").ToList();
        }
    }


    public List<Oar> GetAllOarPointsById(int idStocks)
    {
        using(IDbConnection dbConnection = Connection)
        {
            dbConnection.Open();
            return dbConnection.Query<Oar>($"SELECT OarResult FROM Oars WHERE idStocks = '{idStocks}';").ToList();
        }
    }  

    public Oar GetAllOarDataById(int idStocks)
    {
        using(IDbConnection dbConnection = Connection)
        {
            dbConnection.Open();
            return dbConnection.Query<Oar>($"SELECT * FROM Oars WHERE idStocks = '{idStocks}'").SingleOrDefault();
        }
    }


    public List<Stock> GetAllStocks()
    {
        using (IDbConnection dbConnection = Connection)
        {
            dbConnection.Open();
            return dbConnection.Query<Stock>("SELECT * FROM Stocks").ToList();
        }
    }


    public List<Chart> GetAllTargetCharts()
    {
        using(IDbConnection dbConnection = Connection)
        {
            dbConnection.Open();
            return dbConnection.Query<Chart>("SELECT * FROM Charts").ToList();
        }
    }


    public List<Oar> GetAllOarResults()
    {
        using(IDbConnection dbConnection = Connection)
        {
            dbConnection.Open();
            return dbConnection.Query<Oar>("SELECT * FROM Oars;").ToList();
        }
    }


    public List<OpenVolumeobj> GetAllOpenVolumeResults()
    {
        using(IDbConnection dbConnection = Connection)
        {
            dbConnection.Open();
            return dbConnection.Query<OpenVolumeobj>($"SELECT * FROM OpenVolume;").ToList();
        }
    }



   




    //////////////////////////////////////////////////
    //////////////////////////////////////////////////
    //////////////////////////////////////////////////
            //    DELETE ALL'S 



    public void DeleteAllUsers()
    {
        using (IDbConnection dbConnection = Connection)
        {
            dbConnection.Open();
            dbConnection.Execute("SET foreign_key_checks = 0;");
            dbConnection.Execute("TRUNCATE TABLE users;");
            dbConnection.Execute("TRUNCATE TABLE stocks;");                
            dbConnection.Execute("SET foreign_key_checks = 1;");                
        }
    }

    public void DeleteAllTargetCharts()
    {
        using(IDbConnection dbConnection = Connection)
        {
            dbConnection.Open();
            dbConnection.Execute("TRUNCATE TABLE TargetCharts;");
        }
    }
     


    public void DeleteAllStocks()
    {
        using (IDbConnection dbConnection = Connection)
        {
            dbConnection.Open();
            dbConnection.Execute("SET foreign_key_checks = 0;");
            dbConnection.Execute("TRUNCATE TABLE Stocks;");                
            dbConnection.Execute("SET foreign_key_checks = 1;");                
        }
    }

    public void DeleteAllOars()
    {
        using (IDbConnection dbConnection = Connection)
        {
            dbConnection.Open();
            dbConnection.Execute("SET foreign_key_checks = 0;");
            dbConnection.Execute("TRUNCATE TABLE Oars;");                
            dbConnection.Execute("SET foreign_key_checks = 1;");                
        }
    }
    

    public void DeleteOpenVolume()
    {
        using(IDbConnection dbConnection = Connection)
        {
            dbConnection.Open();
            dbConnection.Execute("TRUNCATE TABLE OpenVolume;");
        }
    }


         
    
    }

}


  // public Stock EditStock(Stock newStockData, int idstocks)
        // {
        //     using (IDbConnection dbConnection = Connection)
        //     {
        //         dbConnection.Open();
        //         dbConnection.Execute($@"UPDATE stocks SET CompanyName = '{newStockData.CompanyName}', StockSymbol = '{newStockData.StockSymbol}', 
        //                                 StockPrice = '{newStockData.StockPrice}', StockDescription = '{newStockData.StockDescription}', 
        //                                 Industry = '{newStockData.Industry}', DrugName = '{newStockData.DrugName}', DrugTreating = '{newStockData.DrugTreating}', 
        //                                 updatedAt = NOW() WHERE idstocks = '{idstocks}'");

        //         return dbConnection.Query<Stock>($"SELECT * FROM stocks WHERE idstocks = '{idstocks}'").SingleOrDefault();                
        //     }
        // }


        // public void DeleteStock(int idstocks)
        // {
        //     using (IDbConnection dbConnection = Connection)
        //     {
        //         dbConnection.Open();
        //         dbConnection.Execute($"DELETE FROM stocks WHERE idstocks = '{idstocks}'");
        //     }
        // }



        




        //    public float MaximumShortSqueezePercentage(int )
    //    {
            
    //    }
