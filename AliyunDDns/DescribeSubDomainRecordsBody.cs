using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliyunDDns
{
    public class DescribeSubDomainRecordsBody
    {
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public string RequestId { get; set; }
        public Domainrecords DomainRecords { get; set; }
        public int PageNumber { get; set; }
    }

    public class Domainrecords
    {
        public Record[] Record { get; set; }
    }

    public class Record
    {
        public string Status { get; set; }
        public string Line { get; set; }
        public string RR { get; set; }
        public bool Locked { get; set; }
        public string Type { get; set; }
        public string DomainName { get; set; }
        public string Value { get; set; }
        public string RecordId { get; set; }
        public int TTL { get; set; }
        public int Weight { get; set; }
    }

}
