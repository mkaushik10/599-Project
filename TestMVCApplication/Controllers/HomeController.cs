using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestMVCApplication.Filters;
using TestMVCApplication.Models;
using System.Data;
using Npgsql;
using System.Configuration;

namespace TestMVCApplication.Controllers
{
    public class HomeController : Controller
    {
        static string currentLat, currentLong;
        NpgsqlConnection conn;

        public HomeController()
        {
            currentLat = string.Empty;
            currentLong = string.Empty;
            NpgsqlConnection conn = null;
        }

        public ActionResult Index()
        {
            return View(new PageViewModel());
        }

        //public ActionResult About()
        //{
        //    ViewBag.Message = "Your app description page.";

        //    return View();
        //}

        //public ActionResult Contact()
        //{
        //    ViewBag.Message = "Your contact page.";

        //    return View();
        //}
        public string GetDataText(int data, char param)
        {
            switch (param)
            {
                case 'P': switch (data)
                    {
                        case 0: return "Sparse population";
                        case 1: return "Medium population";
                        case 2: return "High population";
                        default: return "Medium population";
                    }
                case 'I': switch (data)
                    {
                        case 0: return "Low income";
                        case 1: return "Medium income";
                        case 2: return "High income";
                        default: return "Medium income";
                    }
                case 'E': switch (data)
                    {
                        case 0: return "High employment";
                        case 1: return "Medium employment";
                        case 2: return "Low employment";
                        default: return "Medium employment";
                    }
                case 'C': switch (data)
                    {
                        case 0: return "Safe World";
                        case 1: return "Medium safe";
                        case 2: return "A little shady";
                        default: return "Medium safe";
                    }
                default: return string.Empty;
            }
        }

        [HttpPost]
        [CheckAjaxRequest]
        public ActionResult FetchInformation(string lat, string lng)
        {
            try
            {
                currentLat = lat;
                currentLong = lng;

                //get census data
                string query = string.Format(@"select * from CACensusData_Stats c
                            where st_contains(c.geom,st_point({0},{1}))", lng, lat);
                DataTable dt = getData(query);
                CACensusData censusData = null;
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        censusData = new CACensusData()
                        {
                            population = (dt.Rows[0]["populationestimate"] != DBNull.Value) ? Convert.ToDecimal(dt.Rows[0]["populationestimate"]) : 0,
                            unemployed = (dt.Rows[0]["percentunemployed"] != DBNull.Value) ? Convert.ToDecimal(dt.Rows[0]["percentunemployed"]) : 0,
                            medianHouseholdIncome = (dt.Rows[0]["medianhouseholdincome"] != DBNull.Value) ? Convert.ToDecimal(dt.Rows[0]["medianhouseholdincome"]) : 0,
                            popDensity = (dt.Rows[0]["pop_density"] != DBNull.Value) ? GetDataText(Convert.ToInt32(dt.Rows[0]["pop_density"]), 'P') : string.Empty,
                            unemployment = (dt.Rows[0]["median_income"] != DBNull.Value) ? GetDataText(Convert.ToInt32(dt.Rows[0]["median_income"]), 'I') : string.Empty,
                            medIncome = (dt.Rows[0]["per_unemployment"] != DBNull.Value) ? GetDataText(Convert.ToInt32(dt.Rows[0]["per_unemployment"]), 'E') : string.Empty,
                            crimeRate = (dt.Rows[0]["crimeCount"] != DBNull.Value) ? GetDataText(Convert.ToInt32(dt.Rows[0]["crimeCount"]), 'C') : GetDataText(3,'C'),
                            county = (dt.Rows[0]["county"] != DBNull.Value) ? Convert.ToString(dt.Rows[0]["county"]) : string.Empty
                        };
                    }
                }
                //get coffee outlets
                List<CoffeeOutlets> coffeeOutletsList = GetCoffeeOutlets(lat, lng);

                //return the collected data for plotting on UI
                if (censusData != null)
                {
                    return Json(new
                    {
                        caCensusData = censusData,
                        coffeeOutletsLst = coffeeOutletsList
                    });
                }
            }
            catch (Exception)
            {
            }
            return Json(new { status = false });
        }

        private List<CoffeeOutlets> GetCoffeeOutlets(string lat, string lng)
        {
            string query = string.Format(@"select long,lat,location,address from starbucks s
                        where st_dwithin(st_point({0},{1}),st_point(s.lat,s.long),0.03)
                        union 
                        select lat,long,location,address from independent_coffee c
                        where st_dwithin(st_point({0},{1}),st_point(c.lat,c.long),0.03)", lat, lng);

            //query = "select * from starbucks where address like '%Los An%' limit 10";
            DataTable dtCoffee = getData(query);
            List<CoffeeOutlets> coffeeOutletsList = new List<CoffeeOutlets>();
            if (dtCoffee != null && dtCoffee.Rows.Count > 0)
            {
                for (int i = 0; i < dtCoffee.Rows.Count; i++)
                {
                    string Latitude = Convert.ToString(dtCoffee.Rows[i][1]);
                    string Longitude = Convert.ToString(dtCoffee.Rows[i][0]);
                    string Location = Convert.ToString(dtCoffee.Rows[i][2]);
                    coffeeOutletsList.Add(new CoffeeOutlets() { location = Location, latitude = Latitude, longitude = Longitude });
                }
            }
            return coffeeOutletsList;
        }

        public ActionResult GetCoffeeOutletsJson(string lat, string lng)
        {
            List<CoffeeOutlets> coffeeOutletsList = GetCoffeeOutlets(lat, lng);
            if (coffeeOutletsList != null && coffeeOutletsList.Count > 0)
                return Json(new { coffeeOutletsLst = coffeeOutletsList });
            return Json(new { status = false });
        }

        [HttpPost]
        [CheckAjaxRequest]
        public ActionResult FetchAreaInformation(string geoid)
        {
            try
            {
                string query = string.Format("select ST_AsGeoJSON(geom) as geoJSON,* from CACensusData_Stats where geoid2={0}", geoid);
                DataTable dtAreaInfo = getData(query);
                if (dtAreaInfo != null && dtAreaInfo.Rows.Count > 0)
                {
                    //get data and plot a polygon on map for this geom
                    return Json(new
                    {
                        areaData = new CACensusData()
                            {
                                geoJSON = Convert.ToString(dtAreaInfo.Rows[0]["geoJSON"]),
                                population = (dtAreaInfo.Rows[0]["populationestimate"] != DBNull.Value) ? Convert.ToDecimal(dtAreaInfo.Rows[0]["populationestimate"]) : 0,
                                unemployed = (dtAreaInfo.Rows[0]["percentunemployed"] != DBNull.Value) ? Convert.ToDecimal(dtAreaInfo.Rows[0]["percentunemployed"]) : 0,
                                medianHouseholdIncome = (dtAreaInfo.Rows[0]["medianhouseholdincome"] != DBNull.Value) ? Convert.ToDecimal(dtAreaInfo.Rows[0]["medianhouseholdincome"]) : 0,
                                popDensity = (dtAreaInfo.Rows[0]["pop_density"] != DBNull.Value) ? GetDataText(Convert.ToInt32(dtAreaInfo.Rows[0]["pop_density"]), 'P') : string.Empty,
                                unemployment = (dtAreaInfo.Rows[0]["median_income"] != DBNull.Value) ? GetDataText(Convert.ToInt32(dtAreaInfo.Rows[0]["median_income"]), 'I') : string.Empty,
                                medIncome = (dtAreaInfo.Rows[0]["per_unemployment"] != DBNull.Value) ? GetDataText(Convert.ToInt32(dtAreaInfo.Rows[0]["per_unemployment"]), 'E') : string.Empty,
                                crimeRate = (dtAreaInfo.Rows[0]["crimeCount"] != DBNull.Value) ? GetDataText(Convert.ToInt32(dtAreaInfo.Rows[0]["crimeCount"]), 'C') : string.Empty,
                                county = (dtAreaInfo.Rows[0]["county"] != DBNull.Value) ? Convert.ToString(dtAreaInfo.Rows[0]["county"]) : string.Empty,                               
                                geoid = (dtAreaInfo.Rows[0]["geoid2"] != DBNull.Value) ? Convert.ToInt64(dtAreaInfo.Rows[0]["geoid2"]) : 0,
                                geom = (dtAreaInfo.Rows[0]["geom"] != DBNull.Value) ? Convert.ToString(dtAreaInfo.Rows[0]["geom"]) : string.Empty
                            }
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            { }
            return Json(new { status = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Index(PageViewModel model)
        {
            try
            {
                string query = string.Empty;
                if (string.IsNullOrEmpty(currentLat) && string.IsNullOrEmpty(currentLong))
                {
                    //add crime query too when ready
                    //run query for that particular point and suggest areas around that point.
                    query = string.Format("select * from CACensusData_Stats v where pop_density={0} and median_income={1} and per_unemployment={2} limit 5"
                        , model.selectedPopulation, model.selectedIncome, model.selectedEmployment);
                }
                else
                {
                    //run generic query and suggest top 5 locations with information
                    //think about plotting these geoms as polygons on map with the information on click of it
                    query = string.Format(@"select * from CACensusData_Stats v where pop_density={0} and median_income={1} and per_unemployment={2}
                                 and st_dwithin(st_point({3},{4}),v.geom,0.01) limit 5"
                        , model.selectedPopulation, model.selectedIncome, model.selectedEmployment, currentLat, currentLong);
                }

                DataTable dtsuggestedAreas = getData(query);
                if (dtsuggestedAreas != null && dtsuggestedAreas.Rows.Count > 0)
                {
                    foreach (DataRow drArea in dtsuggestedAreas.Rows)
                    {
                        model.Areas.Add(new CACensusData()
                        {
                            population = (drArea["populationestimate"] != DBNull.Value) ? Convert.ToDecimal(drArea["populationestimate"]) : 0,
                            unemployed = (drArea["percentunemployed"] != DBNull.Value) ? Convert.ToDecimal(drArea["percentunemployed"]) : 0,
                            medianHouseholdIncome = (drArea["medianhouseholdincome"] != DBNull.Value) ? Convert.ToDecimal(drArea["medianhouseholdincome"]) : 0,
                            popDensity = (drArea["pop_density"] != DBNull.Value) ? GetDataText(Convert.ToInt32(drArea["pop_density"]), 'P') : string.Empty,
                            unemployment = (drArea["median_income"] != DBNull.Value) ? GetDataText(Convert.ToInt32(drArea["median_income"]), 'I') : string.Empty,
                            medIncome = (drArea["per_unemployment"] != DBNull.Value) ? GetDataText(Convert.ToInt32(drArea["per_unemployment"]), 'E') : string.Empty,
                            county = (drArea["county"] != DBNull.Value) ? Convert.ToString(drArea["county"]) : string.Empty,
                            crimeRate = (drArea["crimeCount"] != DBNull.Value) ? GetDataText(Convert.ToInt32(drArea["crimeCount"]), 'C') : string.Empty,
                            popChange = (drArea["popchange"] != DBNull.Value) ? Convert.ToInt32(drArea["popchange"]) : 0,
                            geoid = (drArea["geoid2"] != DBNull.Value) ? Convert.ToInt64(drArea["geoid2"]) : 0,
                            geom = (drArea["geom"] != DBNull.Value) ? Convert.ToString(drArea["geom"]) : string.Empty
                        });
                    }
                }
            }
            catch (Exception)
            {
            }
            return View(model);
        }

        private DataTable getData(string query)
        {
            DataTable dt = null;
            conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["PostgresConn"].ConnectionString);
            conn.Open();
            NpgsqlCommand command = new NpgsqlCommand(query, conn);
            try
            {
                dt = new DataTable();
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(command);
                da.Fill(dt);
            }
            catch (Exception e) { }
            finally
            {
                conn.Close();
            }
            return dt;
        }
    }
}
