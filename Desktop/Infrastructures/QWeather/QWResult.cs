using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desktop.Infrastructures.QWeather
{
    /// <summary>
    /// API状态码
    /// </summary>
    public enum QWResultCode
    {
        /// <summary>
        /// 请求成功
        /// </summary>
        OK = 200,

        /// <summary>
        /// 请求成功，但你查询的地区暂时没有你需要的数据。
        /// </summary>
        NoContent = 204,

        /// <summary>
        /// 请求错误，可能包含错误的请求参数或缺少必选的请求参数。
        /// </summary>
        BadRequest = 400,

        /// <summary>
        /// 认证失败，可能使用了错误的KEY、数字签名错误、KEY的类型错误（如使用SDK的KEY去访问Web API）。
        /// </summary>
        Unauthorized = 401,

        /// <summary>
        /// 超过访问次数或余额不足以支持继续访问服务，你可以充值、升级访问量或等待访问量重置。
        /// </summary>
        PaymentRequired = 402,

        /// <summary>
        /// 无访问权限，可能是绑定的PackageName、BundleID、域名IP地址不一致，或者是需要额外付费的数据。
        /// </summary>
        Forbidden = 403,

        /// <summary>
        /// 查询的数据或地区不存在。
        /// </summary>
        NotFound = 404,

        /// <summary>
        /// 超过限定的QPM（每分钟访问次数）
        /// </summary>
        TooManyRequests = 429,

        /// <summary>
        /// 无响应或超时，接口服务异常
        /// </summary>
        InternalServerError = 500
    }

    /// <summary>
    /// API 结果
    /// </summary>
    public record QWResult
    {
        /// <summary>
        /// API状态码
        /// </summary>
        public QWResultCode Code { get; init; }

        /// <summary>
        /// 当前API的最近更新时间
        /// </summary>
        public DateTime UpdateTime { get; init; }

        /// <summary>
        /// 当前数据的响应式页面，便于嵌入网站或应用
        /// </summary>
        public string FxLink { get; init; }
    }

    /// <summary>
    /// 实时天气
    /// </summary>
    public record QWNowData
    {
        /// <summary>
        /// 数据观测时间
        /// </summary>
        public DateTime ObsTime { get; init; }

        /// <summary>
        /// 温度，默认单位：摄氏度
        /// </summary>
        public float Temp { get; init; }

        /// <summary>
        /// 体感温度，默认单位：摄氏度
        /// </summary>
        public float FeelsLike { get; init; }

        /// <summary>
        /// 天气状况和图标的代码，图标可通过天气状况和图标下载
        /// </summary>
        public string Icon { get; init; }

        /// <summary>
        /// 天气状况的文字描述，包括阴晴雨雪等天气状态的描述
        /// </summary>
        public string Text { get; init; }

        /// <summary>
        /// 风向360角度
        /// </summary>
        public float Wind360 { get; init; }

        /// <summary>
        /// 风向
        /// </summary>
        public string WindDir { get; init; }

        /// <summary>
        /// 风力等级
        /// </summary>
        public float WindScale { get; init; }

        /// <summary>
        /// 风速，公里/小时
        /// </summary>
        public float WindSpeed { get; init; }

        /// <summary>
        /// 相对湿度，百分比数值
        /// </summary>
        public float Humidity { get; init; }

        /// <summary>
        /// 当前小时累计降水量，默认单位：毫米
        /// </summary>
        public float Precip { get; init; }

        /// <summary>
        /// 大气压强，默认单位：百帕
        /// </summary>
        public float Pressure { get; init; }

        /// <summary>
        /// 能见度，默认单位：公里
        /// </summary>
        public float Vis { get; init; }

        /// <summary>
        /// 云量，百分比数值。可能为空
        /// </summary>
        public float? Cloud { get; init; }

        /// <summary>
        /// 露点温度。可能为空
        /// </summary>
        public float? Dew { get; init; }

    }

    /// <summary>
    /// 实时天气 API 结果
    /// </summary>
    public record QWNowResult : QWResult
    {
        /// <summary>
        /// 实时天气
        /// </summary>
        public QWNowData Now { get; init; }
    }
}
