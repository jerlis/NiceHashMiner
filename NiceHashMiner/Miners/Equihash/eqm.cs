﻿using NiceHashMiner.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMiner.Miners.Grouping;
using NiceHashMiner.Miners.Parsing;

namespace NiceHashMiner.Miners {
    public class eqm : nheqBase {
        public eqm()
            : base("eqm") {
            Path = MinerPaths.eqm;
            WorkingDirectory = MinerPaths.eqm.Replace("eqm.exe", "");
            IsNHLocked = true;
        }

        public override void Start(string url, string btcAdress, string worker) {
            LastCommandLine = GetDevicesCommandString() + " -a " + APIPort + " -l " + url + " -u " + btcAdress + " -w " + worker;
            ProcessHandle = _Start();
        }


        protected override string GetDevicesCommandString() {
            string deviceStringCommand = " ";

            if (CPU_Setup.IsInit) {
                deviceStringCommand += "-p " + CPU_Setup.MiningPairs.Count;
                deviceStringCommand += " " + ExtraLaunchParametersParser.ParseForMiningSetup(CPU_Setup, DeviceType.CPU);
            } else {
                // disable CPU
                deviceStringCommand += " -t 0 ";
            }

            if (NVIDIA_Setup.IsInit) {
                deviceStringCommand += " -cd ";
                foreach (var nvidia_pair in NVIDIA_Setup.MiningPairs) {
                    for (int i = 0; i < ExtraLaunchParametersParser.GetEqmCudaThreadCount(nvidia_pair); ++i) {
                        deviceStringCommand += nvidia_pair.Device.ID + " ";
                    }
                }
                // no extra launch params
                deviceStringCommand += " " + ExtraLaunchParametersParser.ParseForMiningSetup(NVIDIA_Setup, DeviceType.NVIDIA);
            }

            return deviceStringCommand;
        }

        // benchmark stuff
        protected override bool BenchmarkParseLine(string outdata) {

            if (outdata.Contains(Iter_PER_SEC)) {
                curSpeed = getNumber(outdata, "Measured: ", Iter_PER_SEC) * SolMultFactor;
            }
            if (outdata.Contains(Sols_PER_SEC)) {
                var sols = getNumber(outdata, "Measured: ", Sols_PER_SEC);
                if (sols > 0) {
                    BenchmarkAlgorithm.BenchmarkSpeed = curSpeed;
                    return true;
                }
            }
            return false;
        }
    }
}
