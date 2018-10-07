using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class User
    {
        public int idusers {get; set;}
        [Required]
        [MinLength(2, ErrorMessage="First name must be at least two characters")]
        public string FirstName {get; set;}
        [Required]
        [MinLength(2, ErrorMessage="Last name must be at least two characters long")]
        public string LastName {get; set;}
        [Required]
        [MinLength(2, ErrorMessage="Email must be at least two characters long")]
        public string Email {get; set;}
        public string Password {get; set;}
        public string ConfirmPassword {get; set;}
        public string createdAt {get; set;}
        public string updatedAt {get; set;}

        public class LogUser 
        {
            public string Email {get; set;}
            public string Password {get; set;}
        }

        public static explicit operator User(List<Dictionary<string, object>> v)
        {
            throw new NotImplementedException();
        }
    }

    public class Stock
    {
        public int idStocks {get; set;}
        public int userId {get; set;}
        public string Symbol {get; set;}
        public double Last {get; set;}
        public string CompName {get; set;}
        public float SupPoint {get; set;}
        public float ResPoint {get; set;}
        public float Oar {get; set;}
        public float OarResult {get; set;}
        public double RelativeVolume {get; set;}
        public string Comment {get; set;}
        public string createdAt {get; set;}
        public string updatedAt {get; set;}
        
    }


    public class Notes
    {
        public int idnotes {get; set;}
        public int userId {get; set;}
        public int stockId {get; set;}
        public int Content {get; set;}
        public string createdAt {get; set;}
        public string updatedAt {get; set;}
    }

    public class Float_Volume
    {
        public double RelativeFloat {get; set;}
        public double Volume {get; set;}
        public double Result {get; set;}

        public class Provided_Float_Volume
        {
            public double providedRange {get; set;} 
            public double providedFloat {get; set;}
            public double providedVolume {get; set;}                                        
        }
    }


    public class Oar
    {
        public int idStocks {get; set;}
        public float ResPoint {get; set;}
        public float SupPoint {get; set;}
        public float OarResult {get; set;}
        public string createdAt {get; set;}
        public string updatedAt {get; set;}
    }



    public class Playerlevels 
    {
        public int Playerlevel {get; set;}
        public int AmountofTrades {get; set;}
        public int MaxLoss {get; set;}
        public int riskLossRatio {get; set;}
        public int AverageStockPrice {get; set;}

    }


    public class Chart
    {
        public int idCharts {get; set;}
        public int userCost {get; set;}
        public double stockPrice {get; set;}
        public List<double> stockPriceList {get; set;}
        public List<double> shareSizeList {get; set;}
        public List<double> targetList {get; set;}
        public List<double> winningsList {get; set;}
        public string createdAt {get; set;}
    }
    
    
}
