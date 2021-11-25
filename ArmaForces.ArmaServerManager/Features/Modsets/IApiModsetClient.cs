using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Features.Modsets.DTOs;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Modsets {
    /// <summary>
    /// Handles connection and retrieving modsets data from web API.
    /// </summary>
    public interface IApiModsetClient {
        /// <summary>
        /// Retrieves all modsets available.
        /// </summary>
        /// <returns><see cref="List{T}"/> of <see cref="WebModset"/></returns>
        Task<Result<List<WebModset>>> GetModsets();

        /// <summary>
        /// Retrieves modset data from API by it's name.
        /// </summary>
        /// <param name="name">Modset name.</param>
        /// <exception cref="HttpRequestException"></exception>
        /// <returns><seealso cref="WebModset"/></returns>
        Task<Result<WebModset>> GetModsetDataByName(string name);

        /// <summary>
        /// Retrieves data for given <see cref="WebModset"/> from API.
        /// </summary>
        /// <param name="webModset"><see cref="WebModset"/> object.</param>
        /// <exception cref="HttpRequestException"></exception>
        /// <returns><seealso cref="WebModset"/></returns>
        Task<Result<WebModset>> GetModsetDataByModset(WebModset webModset);

        /// <summary>
        /// Retrieves modset data from API by it's ID.
        /// </summary>
        /// <param name="id">Modset ID</param>
        /// <exception cref="HttpRequestException"></exception>
        /// <returns><seealso cref="WebModset"/></returns>
        Task<Result<WebModset>> GetModsetDataById(string id);
    }
}