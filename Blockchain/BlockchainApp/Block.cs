using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainApp
{
    class Block
    {
        public UInt64 BlockIndex { get; set; }
        public UInt64 UnixTimestamp { get; set; }
        public String PreviousBlockHash { get; set; }
        public UInt64 ProofOfWork { get; set; }
        public List<Record> Records { get; set; }
    }
}
