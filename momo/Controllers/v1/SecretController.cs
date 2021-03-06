﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using momo.Application.Authorization.Jwt;
using momo.Application.Authorization.Jwt.Dto;
using momo.Application.Authorization.Secret;
using momo.Application.Authorization.Secret.Dto;

namespace momo.Controllers.v1
{
    /// <summary>
    /// 授权接口
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize(Policy = "Permission")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [EnableCors("any")]
    public class SecretController : ControllerBase
    {
        #region Initialize

        /// <summary>
        /// Jwt 服务
        /// </summary>
        private readonly IJwtAppService _jwtApp;

        /// <summary>
        /// 日志记录服务
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// 用户服务
        /// </summary>
        private readonly ISecretAppService _secretApp;

        /// <summary>
        /// 配置信息
        /// </summary>
        public IConfiguration _configuration { get; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        /// <param name="jwtApp"></param>
        /// <param name="secretApp"></param>
        public SecretController(ILogger<SecretController> logger, IConfiguration configuration,
            IJwtAppService jwtApp, ISecretAppService secretApp)
        {
            _configuration = configuration;
            _jwtApp = jwtApp;
            _secretApp = secretApp;
            _logger = logger;
        }
        #endregion

        #region APIs

        /// <summary>
        /// 停用 Jwt 授权数据
        /// </summary>
        /// <returns></returns>
        [HttpPost("deactivate")]
        public async Task<IActionResult> CancelAccessToken()
        {
            await _jwtApp.DeactivateCurrentAsync();
            return Ok();
        }

        /// <summary>
        /// 获取 Jwt 授权数据
        /// </summary>
        /// <param name="dto">授权用户信息</param>
        [HttpPost("token")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAsync(SecretDto dto)
        {
            //Todo：获取用户信息
            //var user = new UserDto
            //{
            //    Id = Guid.NewGuid(),
            //    UserName = "yuiter",
            //    Role = Guid.Empty,
            //    Email = "yuiter@yuiter.com",
            //    Phone = "13912345678",
            //};
            var user = await _secretApp.GetCurrentUserAsync(dto.UserName, dto.Password);

            if (user == null)
                return Ok(new JwtResponseDto
                {
                    Access = "用户不存在,无权访问",
                    Type = "Bearer",
                    err_code = 1,
                    Data = new Profile
                    {
                        Name = dto.UserName,
                        Auths = 0,
                        Expires = 0
                    }
                });

            var jwt = _jwtApp.Create(user);

            return Ok(new JwtResponseDto
            {
                Access = jwt.Token,
                Type = "Bearer",
                err_code=0,
                Data = new Profile
                {
                    Name = user.UserName,
                    Auths = jwt.Auths,
                    Expires = jwt.Expires
                }
            });
        }

        /// <summary>
        /// 刷新 Jwt 授权数据
        /// </summary>
        /// <param name="dto">刷新授权用户信息</param>
        /// <returns></returns>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshAccessTokenAsync([FromBody] SecretDto dto)
        {

            var auth = HttpContext.AuthenticateAsync().Result.Principal.Claims;
            //Todo：获取用户信息
            //var user = new UserDto
            //{
            //    Id = Guid.NewGuid(),
            //    UserName = "yuiter",
            //    Role = Guid.Empty,
            //    Email = "yuiter@yuiter.com",
            //    Phone = "13912345678",
            //};
            var user = await _secretApp.GetCurrentUserAsync(dto.UserName, dto.Password);

            if (user == null)
                return Ok(new JwtResponseDto
                {
                    Access = "无权访问",
                    Type = "Bearer",
                    err_code = 1,
                    Data = new Profile
                    {
                        Name = dto.UserName,
                        Auths = 0,
                        Expires = 0
                    }
                });

            var jwt = await _jwtApp.RefreshAsync(dto.Token, user);

            return Ok(new JwtResponseDto
            {
                Access = jwt.Token,
                err_code = 0,
                Type = "Bearer",
                Data = new Profile
                {
                    Name = user.UserName,
                    Auths = jwt.Success ? jwt.Auths : 0,
                    Expires = jwt.Success ? jwt.Expires : 0
                }
            });
        }

        #endregion
    }
}