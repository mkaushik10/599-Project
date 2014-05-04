using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace TestMVCApplication.Models
{
    public class CACensusData
    {
        public decimal population { get; set; }
        public decimal medianHouseholdIncome { get; set; }
        public decimal unemployed { get; set; }
        public string county { get; set; }
        public string geom { get; set; }
        public string popDensity { get; set; }
        public string medIncome { get; set; }
        public string unemployment { get; set; }
        public string crimeRate { get; set; }
        public int popChange { get; set; }
        public long geoid { get; set; }
        public string geoJSON { get; set; }
    }
    public class CoffeeOutlets
    {
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string addreess { get; set; }
        public string location { get; set; }
    }
    public class PageViewModel
    {
        [Display(Name = "Safe: ")]
        public int selectedCrime { get; set; }
        public SelectList CrimeList { get; set; }

        [Display(Name = "Populated: ")]
        public int selectedPopulation { get; set; }
        public SelectList PopulationList { get; set; }

        [Display(Name = "Employment: ")]
        public int selectedEmployment { get; set; }
        public SelectList EmploymentList { get; set; }

        [Display(Name = "Income: ")]
        public int selectedIncome { get; set; }
        public SelectList IncomeList { get; set; }

        public List<CACensusData> Areas { get; set; }
        public string selectedLat { get; set; }
        public string selectedLong { get; set; }

        public PageViewModel()
        {
            Areas = new List<CACensusData>();
            List<SelectListItem> crimeList = new List<SelectListItem>();
            crimeList.Add(new SelectListItem()
            {
                Value = "2",
                Text = "Highly Safe"
            });
            crimeList.Add(new SelectListItem()
            {
                Value = "1",
                Text = "Moderate Safe"
            });
            crimeList.Add(new SelectListItem()
            {
                Value = "0",
                Text = "Safe!!"
            });
            CrimeList = new SelectList(crimeList, "Value", "Text");

            List<SelectListItem> popList = new List<SelectListItem>();
            popList.Add(new SelectListItem()
            {
                Value = "2",
                Text = "High"
            });
            popList.Add(new SelectListItem()
            {
                Value = "1",
                Text = "Moderate"
            });
            popList.Add(new SelectListItem()
            {
                Value = "0",
                Text = "Low"
            });
            PopulationList = new SelectList(popList, "Value", "Text");

            List<SelectListItem> employmentList = new List<SelectListItem>();
            employmentList.Add(new SelectListItem()
            {
                Value = "2",
                Text = "High"
            });
            employmentList.Add(new SelectListItem()
            {
                Value = "1",
                Text = "Moderate"
            });
            employmentList.Add(new SelectListItem()
            {
                Value = "0",
                Text = "Low"
            });
            EmploymentList = new SelectList(employmentList, "Value", "Text");

            List<SelectListItem> incomeList = new List<SelectListItem>();
            incomeList.Add(new SelectListItem()
            {
                Value = "2",
                Text = "High"
            });
            incomeList.Add(new SelectListItem()
            {
                Value = "1",
                Text = "Moderate"
            });
            incomeList.Add(new SelectListItem()
            {
                Value = "0",
                Text = "Low"
            });
            IncomeList = new SelectList(incomeList, "Value", "Text");
        }
    }
}
