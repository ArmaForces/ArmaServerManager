﻿using System.Collections.Generic;
using System.Net.Http;
using ArmaForces.ArmaServerManager.Features.Modsets.DTOs;

namespace ArmaForces.ArmaServerManager.Features.Modsets {
    /// <summary>
    /// Handles connection and retrieving modsets data from web API.
    /// </summary>
    public interface IApiModsetClient {
        /// <summary>
        /// Retrieves all modsets available.
        /// </summary>
        /// <returns><see cref="List{T}"/> of <see cref="WebModset"/></returns>
        List<WebModset> GetModsets();

        /// <summary>
        /// Retrieves modset data from API by it's name.
        /// </summary>
        /// <param name="name">Modset name.</param>
        /// <exception cref="HttpRequestException"></exception>
        /// <returns><seealso cref="WebModset"/></returns>
        WebModset GetModsetDataByName(string name);

        /// <summary>
        /// Retrieves data for given <see cref="WebModset"/> from API.
        /// </summary>
        /// <param name="webModset"><see cref="WebModset"/> object.</param>
        /// <exception cref="HttpRequestException"></exception>
        /// <returns><seealso cref="WebModset"/></returns>
        WebModset GetModsetDataByModset(WebModset webModset);

        /// <summary>
        /// Retrieves modset data from API by it's ID.
        /// </summary>
        /// <param name="id">Modset ID</param>
        /// <exception cref="HttpRequestException"></exception>
        /// <returns><seealso cref="WebModset"/></returns>
        WebModset GetModsetDataById(string id);
    }
}