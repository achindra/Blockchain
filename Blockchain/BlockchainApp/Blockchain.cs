using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace BlockchainApp
{
    class Blockchain
    {
        private List<Block>  _BlockChain  = new List<Block>();
        private List<Record> _CurrRecords = new List<Record>();
        private HashSet<Uri> _MinerNodes = new HashSet<Uri>();

        public string MyMinerNodeId = Guid.NewGuid().ToString();

        private void RegisterMiningNodes(Uri MinerId)
        {
            _MinerNodes.Add(MinerId);
        }

        public Blockchain()
        {
            // Genesis Block
            CreateBlock(1, "1");
        }

        public Block CreateBlock(UInt64 ProofOfWork, String PreviousBlockHash = null)
        {
            _CurrRecords.Clear();

            Block block = new Block
            {
                BlockIndex = (UInt64)_BlockChain.Count,
                UnixTimestamp = (UInt64)DateTimeOffset.Now.ToUnixTimeSeconds(),
                ProofOfWork = ProofOfWork,
                PreviousBlockHash = PreviousBlockHash ?? FindHash(_BlockChain.Last()),
                Records = _CurrRecords
            };

            _BlockChain.Add(block);

            return block;
        }

        public List<Block> GetBlockChain(Uri dummy)
        {
            return _BlockChain;
        }
            
                
        public UInt64 AddRecord(DateTime timeStamp, String user, String command)
        {
            var Record = new Record
            {
                CommandTimeStamp = timeStamp,
                UserOnTerminal = user,
                CommandExecuted = command
            };

            _CurrRecords.Add(Record);

            // Return index of next block
            return _BlockChain.Last().BlockIndex + 1;
        }

        private UInt64 GenerateProofOfWork(UInt64 PrevProofOfWork, string PreviousBlockHash)
        {
            UInt64 proofOfWork = 0;
            string hashBytes = Convert.ToString(PrevProofOfWork) +
                                    Convert.ToString(proofOfWork) +
                                    PreviousBlockHash;

            while (!IsValidPoW(hashBytes))
                proofOfWork++;

            return proofOfWork;
        }

        private bool IsValidPoW(string hashBytes)
        {
            return GetHash(hashBytes).EndsWith("C3");
        }

        private string FindHash(Block block)
        {
            return GetHash(JsonConvert.SerializeObject(block));
        }

        private string GetHash(string strBytes)
        {
            SHA256Managed hashString = new SHA256Managed();
            StringBuilder str = new StringBuilder();

            byte[] bytes = Encoding.Unicode.GetBytes(strBytes);
            byte[] byteArray = hashString.ComputeHash(bytes);
            foreach (byte b in byteArray)
            {
                str.AppendFormat("{0:x2}", b);
            }
            return str.ToString();
        }

        private bool IsValidBlockChain(List<Block> chain)
        {
            int index = 1;
            Block prevBlock = chain.First();

            while(index < chain.Count)
            {
                Block block = chain.ElementAt(index);
                if(block.PreviousBlockHash != FindHash(prevBlock))
                {
                    return false;
                }

                string hashBytes = Convert.ToString(prevBlock.ProofOfWork) +
                                    Convert.ToString(block.ProofOfWork) +
                                    prevBlock.PreviousBlockHash;
                if (!IsValidPoW(hashBytes))
                {
                    return false;
                }

                prevBlock = block; index++;
            }

            return true;
        }

        private bool ResolveConflict()
        {
            List<Block> tmpChain = _BlockChain;
            foreach(Uri node in _MinerNodes)
            {
                List<Block> blockChain = GetBlockChain(node);
                if((blockChain.Count > tmpChain.Count) &&
                    IsValidBlockChain(blockChain))
                {
                    tmpChain = blockChain;
                }
            }

            if(tmpChain != _BlockChain)
            {
                _BlockChain = tmpChain;
                return true;
            }

            return false;
        }
    }
}
