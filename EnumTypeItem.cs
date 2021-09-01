using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RolsynAnalysis
{
    public class EnumTypeItem
    {
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
        public bool IsRange { get; set; }
        public int Id { get; set; }
        public List<EnumTypeDetailItem> details { get; set; }
        public EnumTypeItem(int Id, string ShortDescription, string LongDescription, bool IsRange)
        {
            this.ShortDescription = ShortDescription;
            this.LongDescription = LongDescription;
            this.IsRange = IsRange;
            this.Id = Id;
            details = new List<EnumTypeDetailItem>();
        }

        public static List<EnumTypeItem> getList()
        {
            List<EnumTypeItem> li = new List<EnumTypeItem>();
            List<string> s = new List<string>()
            {
            "Documentation Type",
            "Bank Statements",
            "Loan Purpose",
            "Property Type",
            "Appraisal Type",
            "Comps",
            "Location",
            "DTI",
            "LTV",
            "Credit Score"
            };
            List<string> l = new List<string>()
            {
            "Type of documentation used to verify income",
            "Number of months bank statements are available",
            "Reason for this Loan",
            "Property Type",
            "Appraisal type",
            "Number of comparable properties included in the appraisal",
            "Location",
            "Debt to Income Ratio",
            "Loan to Value Ratio",
            "The middle credit score for the primary borrower"
            };
            bool range = false;
            for (int i = 1; i < 11; i++)
            {
                if (i == 8)
                    range = true;
                EnumTypeItem e = new EnumTypeItem(i, s[i - 1], l[i - 1], range);
                e.details = EnumTypeDetailItem.getList(i);
                li.Add(e);
            }
            return li;
        }
    }
    public class EnumTypeDetailItem
    {
        public int LoanCodeId { get; set; }
        public int LoanCodeTypeId { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
        public EnumTypeDetailItem(int LoanCodeId, int LoanCodeTypeId, string ShortDescription)
        {
            this.LoanCodeId = LoanCodeId;
            this.LoanCodeTypeId = LoanCodeTypeId;
            this.ShortDescription = ShortDescription;
            this.LongDescription = ShortDescription;
        }
        public static List<EnumTypeDetailItem> getList()
        {
            List<EnumTypeDetailItem> li = new List<EnumTypeDetailItem>();
            li.Add(new EnumTypeDetailItem(1, 1, "Full Doc"));
            li.Add(new EnumTypeDetailItem(1, 2, "Lite Doc"));
            li.Add(new EnumTypeDetailItem(1, 3, "No Doc"));
            li.Add(new EnumTypeDetailItem(2, 4, "6 Months"));
            li.Add(new EnumTypeDetailItem(2, 5, "12 Months"));
            li.Add(new EnumTypeDetailItem(2, 6, "24 Months"));
            li.Add(new EnumTypeDetailItem(3, 7, "Purchase"));
            li.Add(new EnumTypeDetailItem(3, 8, "Cash Out Refinance"));
            li.Add(new EnumTypeDetailItem(3, 9, "Debt Consolidation"));
            li.Add(new EnumTypeDetailItem(4, 10, "Detached"));
            li.Add(new EnumTypeDetailItem(4, 11, "Semi Detached"));
            li.Add(new EnumTypeDetailItem(4, 12, "Townhome"));
            li.Add(new EnumTypeDetailItem(4, 13, "Condo"));
            li.Add(new EnumTypeDetailItem(4, 14, "Brownstone"));
            li.Add(new EnumTypeDetailItem(5, 15, "Tax Appraisal"));
            li.Add(new EnumTypeDetailItem(5, 16, "Market Comparison"));
            li.Add(new EnumTypeDetailItem(5, 17, "Drive By Appraisal"));
            li.Add(new EnumTypeDetailItem(5, 18, "Full Walk Through"));
            li.Add(new EnumTypeDetailItem(6, 19, "1"));
            li.Add(new EnumTypeDetailItem(6, 20, "2"));
            li.Add(new EnumTypeDetailItem(6, 21, "3"));
            li.Add(new EnumTypeDetailItem(7, 22, "Urban"));
            li.Add(new EnumTypeDetailItem(7, 23, "Suburban"));
            li.Add(new EnumTypeDetailItem(7, 24, "Rural"));
            li.Add(new EnumTypeDetailItem(8, 25, "DTI"));
            li.Add(new EnumTypeDetailItem(9, 26, "LTV"));
            li.Add(new EnumTypeDetailItem(10, 27, "Credit Score"));

            return li;
        }

        public static List<EnumTypeDetailItem> getList(int id)
        {
            return getList().Where(p => p.LoanCodeId == id).ToList();
        }
    }

    public class GreetingRuleDetail
    {
        public int GreetingRuleId { get; set; }
        public int? HourMin { get; set; }
        public int? HourMax { get; set; }
        public int? Gender { get; set; }
        public int? MaritalStatus { get; set; }
        public string Greeting { get; set; }

        public GreetingRuleDetail(int GreetingRuleId, int? HourMin, int? HourMaxm, int? Gender, int? MaritalStatus, string Greeting)
        {
            this.GreetingRuleId = GreetingRuleId;
            this.HourMin = HourMin;
            this.HourMax = HourMaxm;
            this.Gender = Gender;
            this.MaritalStatus = MaritalStatus;
            this.Greeting = Greeting;
        }
        public static List<GreetingRuleDetail> getList()
        {
            List<GreetingRuleDetail> li = new List<GreetingRuleDetail>();
            li.Add(new GreetingRuleDetail(1, null, 11, 1, null, "Good Morning Mr."));
            li.Add(new GreetingRuleDetail(2, null, 11, 2, 1, "Good Morning Mrs."));
            li.Add(new GreetingRuleDetail(3, null, 11, 2, 2, "Good Morning Ms."));
            li.Add(new GreetingRuleDetail(4, 12, 17, 1, null, "Good Afternoon Mr."));
            li.Add(new GreetingRuleDetail(5, 12, 17, 2, 1, "Good Afternoon Mrs."));
            li.Add(new GreetingRuleDetail(6, 12, 17, 2, 2, "Good Afternoon Ms."));
            li.Add(new GreetingRuleDetail(7, 18, 22, 1, null, "Good Evening Mr."));
            li.Add(new GreetingRuleDetail(8, 18, 22, 2, 1, "Good Evening Mrs."));
            li.Add(new GreetingRuleDetail(9, 18, 22, 2, 2, "Good Evening Ms."));
            li.Add(new GreetingRuleDetail(10, 23, null, 1, null, "Good Night Mr."));
            li.Add(new GreetingRuleDetail(11, 23, null, 2, 1, "Good Night Mrs."));
            li.Add(new GreetingRuleDetail(12, 23, null, 2, 2, "Good Night Mrs."));

            return li;
        }
    }

    public interface IGreetingProfile
    {
        int Hour { get; set; }
        int Gender { get; set; }
        int MaritalStatus { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
    }

    public class LoanCodes
    {
        public int Code4 { get; set; }
        public int Sequence { get; set; }
        public int LoanCodeTypeId { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
        public string RuleName { get; set; }
        public bool IsRange { get; set; }
        public List<LoanCodeType> details { get; set; }
        public LoanCodes(int Code4, int Sequence, int LoanCodeTypeId, string ShortDescription, string LongDescription, bool IsRange)
        {
            this.Code4 = Code4;
            this.Sequence = Sequence;
            this.LoanCodeTypeId = LoanCodeTypeId;
            this.ShortDescription = ShortDescription;
            this.LongDescription = LongDescription;
            this.IsRange = IsRange;
            this.RuleName = ShortDescription;
            details = new List<LoanCodeType>();
        }
        public static List<LoanCodes> getlist()
        {
            List<LoanCodes> li = new List<LoanCodes>();
            li.Add(new LoanCodes(1, 1, 11, "ShortDescription1", "LongDescription1", false));
            li.Add(new LoanCodes(2, 2, 12, "ShortDescription2", "LongDescription2", false));
            li[0].details.Add(new LoanCodeType(11, false,1,11,1));
            li[1].details.Add(new LoanCodeType(12, false, 2,12,2));
            return li;
        }
    }

    public class LoanCodeType
    {
        public LoanCodeType(int LoanCodeType,bool IsRange,int LoanCodeId, int? Max, int? Min)
        {
            this.IsRange = IsRange;
            this.LoanCodeTypeId = LoanCodeTypeId;
            this.LoanCodeId = LoanCodeId;
            this.Max = Max;
            this.Min = Min;
        }
        public int LoanCodeId { get; set; }
        public int LoanCodeTypeId { get; set; }
        public bool IsRange { get; set; }
        public int? Max { get; set; }
        public int? Min { get; set; }
    }

    public interface ILoanCodes
    {
        int Code4 { get; set; }
        int Sequence { get; set; }
        int LoanCodeTypeId { get; set; }
        string ShortDescription { get; set; }
        string LongDescription { get; set; }
        bool IsRange { get; set; }


    }
}