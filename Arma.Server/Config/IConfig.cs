﻿using CSharpFunctionalExtensions;

namespace Arma.Server.Config {
    public interface IConfig {
        /// <summary>
        /// Path to config basic.cfg file.
        /// </summary>
        string BasicCfg { get; }
        
        /// <summary>
        /// Path to common.json file.
        /// </summary>
        string ConfigJson { get; }

        /// <summary>
        /// Path to config directory.
        /// </summary>
        string DirectoryPath { get; }

        /// <summary>
        /// Path to config server.cfg file.
        /// </summary>
        string ServerCfg { get; }

        /// <summary>
        /// Performs config loading.
        /// </summary>
        /// <returns>Result</returns>
        Result LoadConfig();
    }
}