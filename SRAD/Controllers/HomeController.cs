using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Factories;
using Microsoft.AspNetCore.Mvc;
using Models;
using SRAD.Models;
using Connection;
using Microsoft.AspNetCore.Http;
using static Models.User;
using static Models.F;
using Microsoft.AspNetCore.Mvc.ModelBinding;


namespace SRAD.Controllers
{
    public class HomeController : Controller
    {
        private Float_VolumeFactories _factory;
        public HomeController()
        {
            _factory = new Float_VolumeFactories();
        }

        
           
        public IActionResult Index()
        {
            return View("DimensionsHome");
        }



        [Route("RenderHome")]
        public IActionResult RenderHome()
        {
            // ViewBag.User = _factory.GetUserById((int)HttpContext.Session.GetInt32("sessionId"));
            ViewBag.StockList = _factory.GetAllStocks();
            return View("DimensionsHome");
        }

        [Route("ShowRegister")]
                public IActionResult ShowRegisterView()
                {          
                    return View("RegisterView");
                }


        [Route("ShowFloatVolumeView")]
        public IActionResult ShowFloatVolumeView()
        {
            ViewBag.AllFloatVolumeResults = _factory.GetAllFloatVolumeResults();
            return View("FloatVolumeView");
        }

        [Route("FloVolInfoGram")]
        public IActionResult ShowFloVolInfoGram()
        {
            return View("FloVolInfoGram");
        }

        [Route("ShowAllFloatVolumeRecords")]
        public IActionResult ShowAllFloatVolumeRecords()
        {
            ViewBag.AllResults = _factory.GetAllFloatVolumeResults();
            return View("AllFloatVolumeResultsView");
        }



        // Passing the stringlt ShowStockCreateView() symbol is optional for adding multiple res and sup data points. 
        [Route("ShowOARView")]
        public IActionResult ShowOARView()
        {
            ViewBag.AllStocks = _factory.GetAllStocks();
            return View("OARView");
        }



        [Route("/ShowCreateProject")]
        public IActionResult ShowCreateProjectView()
        {
            return View("CreateProjectView");
        }

        [Route("ShowAboutUsView")]
        public IActionResult ShowAboutUsView()
        {
            return View("AboutUsView");
        }


        [Route("ShowUserDashboard")]
        public IActionResult ShowUserDashboard()
        {
            return View("UserDashboardView");
        }


        [Route("ShowPriorityView")]
        public IActionResult ShowPriorityView()
        {
            return View("PriorityClassView");
        }

        [Route("ShowMainInfoView")]
        public IActionResult ShowMainInfoView()
        {
            return View("MainInfoView");
        }


        [Route("ShowStockView/{idStocks}")]
        public IActionResult ShowStockView(int idStocks)
        {
            ViewBag.Stock = _factory.GetStockById(idStocks);
            ViewBag.SupPoints = _factory.GetAllSupPointsById(idStocks);
            ViewBag.ResPoints = _factory.GetAllResPointsById(idStocks);
            ViewBag.OarPoints = _factory.GetAllOarPointsById(idStocks);          
                      
            return View("StockView");
        }


        [Route("ShowStockCreateView")]
        public IActionResult
        {
            return View("StockCreateView");
        }
        
        [Route("/Home/ShowStockCreateView")]
        public IActionResult ShowStockCreateViewHome()
        {
            return View("StockCreateView");
        }

        [Route("ShowAllOARResultsView")]
        public IActionResult ShowAllOARResultsView()
        {
            ViewBag.AllOARResults = _factory.GetAllStocks();
            return View("AllOARResultsView");
        }

        [Route("ShowTargetChartView")]
        public IActionResult ShowTargetChartView()
        {
            return View("TargetChartView");
        }

        [Route("ShowAllTargetCharts")]
        public IActionResult ShowAllTargetCharts()
        {
            ViewBag.AllTargetCharts = _factory.GetAllTargetCharts();
            return View("ShowAllTargetCharts");
        }

        [Route("DeleteAllTargetCharts")]
        public IActionResult DeleteAllTargetCharts()
        {
            _factory.DeleteAllTargetCharts();
            return View("TargetChartView");
        }




        [HttpPost("/Register")]
        public IActionResult Register(User newUser)
        {
            if(ModelState.IsValid){
                if(_factory.EmailCheck(newUser.Email) == false){
                    _factory.CreateUser(newUser);
                    HttpContext.Session.SetInt32("sessionId", (int)_factory.GetIDFromEmail(newUser.Email));
                    return RedirectToAction("RenderHome");
                }
                else{
                    ModelState.AddModelError("Email", "Account already exists");
                    return View("RegisterView");
                }       
            }
            return View("RegisterView");
        }

        [HttpPost("Login")]
        public IActionResult Login(LogUser loguser)
        {
            if(ModelState.IsValid){
                if(_factory.EmailCheck(loguser.Email)){
                    if(_factory.PasswordCheck(loguser)){
                        User loggedUser = _factory.GetUserByEmail(loguser.Email);
                        HttpContext.Session.SetInt32("sessionId", loggedUser.idusers);
                        return RedirectToAction("RenderHome");
                    }
                    else{
                        ModelState.AddModelError("Password", "Password does not match our records");
                        return View("Index");
                    }
                }
                else{
                    ModelState.AddModelError("Email", "Account does not exist in our records");
                    return View("Index");
                }
            }
            else
                return View("Index");
        }

        [HttpPost("/Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }



        [Route("SymbolCheck")]
        public IActionResult SymbolCheck(Stock stock)
        {
            if(_factory.StockExists(stock.Symbol)){
                ModelState.AddModelError("Symbol", "This symbol does not exist in the records. Please create a record first.");
                return View("OARView");
            }
            else{
                _factory.UpdateResAndSupPoints(stock.Symbol, stock.ResPoint, stock.SupPoint);
                ViewBag.StockResult = _factory.GetStockBySymbol(stock.Symbol);
                return View("OARView");
            }
        }


        [HttpPost("CreateRecord")]
        public IActionResult CreateRecord(Stock stock)
        {
            if(ModelState.IsValid){
                if(_factory.StockExists(stock.Symbol)){
                    ModelState.AddModelError("Symbol", "Record for this symbol already exists. Please check records to update.");
                    return View("StockCreateView");
                }
                else{
                    _factory.CreateRecord(stock);
                    return View("OARView");
                }
            }
            else{
                ModelState.AddModelError("Symbol", "Please correct data points");
                return View("StockCreateView");
            }
        }



        



        // Add Resistance and Support Points for Symbol Relevant historic data 
        [Route("/UpdateResAndSupPoints")]
        public IActionResult UpdateResAndSupPoints(Stock stock)
        {
            if(_factory.StockExists(stock.Symbol)){
                _factory.UpdateResAndSupPoints(stock.Symbol, stock.ResPoint, stock.SupPoint);
                return RedirectToAction("ShowOarView");
            }
            else{
                ModelState.AddModelError("Symbol", "This stock does not yet exist. Please create a new record for the symbol");
                return View("ShowOarView");                
            }
        }

        
        //// The Flo Vol Calculator        
        [Route("CalculateFloVol")]
        public IActionResult CalculateFloVol(ProvidedFloatVolume obj)
        {
            ViewBag.Results = (obj.providedVolume / obj.providedRange);
            return View("FloVolResultsView");
        }




        // This calculates and creates records in the database
        [Route("GenFloVolEquation")]
        public IActionResult GenFloVolEquation(int providedRange)
        {
            _factory.CreateFloVolTable(providedRange);
            return View("FloatVolumeView");
        }



        // This will query the records for a specified range of the fixed baseline FLOAT

        [Route("ShowHistoricFloVolByRequestedFloat")]
        public IActionResult ShowHistoricFloVolByRequestedFloat(int floatRequested)
        {
            ViewBag.Results =  _factory.GetRecordsByFloatRequest(floatRequested);
            return View("FloVolumeByRequestView");
        }


        // [Route("CalcPlayerLevelResults")]

        // public IActionResult CalcPlayerLevelResults(int playerLevel)
        // {
        //     if(_factory.PlayerLevelExists(playerLevel)){
        //         ModelState.AddModelError("PlayerLevel", "Player level records already exists");
        //         return View("PlayerLevels");
        //     }
        //     else{
        //        _factory.CalcPlayerLevelResults(playerLevel); 
        //         ViewBag.PlayerLevelResults = _factory.GetAllPlayerLevels();
        //         return View("PlayerLevels");            
        //     }
        // }


        [Route("calculateTargetChart")]
        public IActionResult calcTargetChart(Chart newChart)
        {   
            ViewBag.Chart = _factory.calcTargetChart(newChart.stockPrice, newChart.userCost);
            return View("ShowTargetChart");
        }



      



        


        


        // These are only here to test data entry and setup once application is working efficiently will delete



        [Route("TruncateStockandOarTable")]
        public IActionResult TruncateStockandOarTable()
        {
            _factory.DeleteAllStocks();
            _factory.DeleteAllOars();
            return View("OARView");
        }


        [Route("DeleteFloatVolume")]
        public IActionResult DeleteFloatVolume()
        {
            _factory.DeleteFloatVolume();
            return RedirectToAction("ShowFloatVolView");
        }




        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }




    }
}










        


        // [Route("/DeleteStock/{idstocks}")]
        // public IActionResult DeleteStock(int idstocks)
        // {
        //     _factory.DeleteStock(idstocks);
        //     return RedirectToAction("RenderHome");
        // }


        // [Route("DeleteAllStocks")]
        // public IActionResult DeleteAllStocks()
        // {
        //     _factory.DeleteAllStocks();
        //     return RedirectToAction("RenderHome");
        // }


        // [Route("ShowAll")]
        // public IActionResult ShowAll()
        // {
        //     ViewBag.AllUsers = _factory.GetAllUsers();
        //     ViewBag.AllStocks = _factory.GetAllStocks();
        //     return View("ShowAll");
        // }
