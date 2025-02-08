using System.ComponentModel;
using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;

namespace BaseSKLearn.SKOfficialDemos.GettingStartedWithProcesses.Step04.Plugins;

/// <summary>
/// 表示一个日历事件的记录类型。
/// </summary>
internal sealed record CalendarEvent(
    /// <summary>
    /// 事件标题
    /// </summary>
    string Title,
    /// <summary>
    /// 事件开始时间，格式字符串
    /// </summary>
    string Start,
    /// <summary>
    /// 事件结束时间，格式字符串，可为空
    /// </summary>
    string? End,
    /// <summary>
    /// 事件描述，可为空
    /// </summary>
    string? Description = null
)
{
    /// <summary>
    /// 以 <see cref="DateTime"/> 类型表示的事件开始日期，此属性在 JSON 序列化时忽略。
    /// </summary>
    [JsonIgnore]
    public DateTime StartDate { get; } = DateTime.Parse(Start);

    /// <summary>
    /// 以 <see cref="DateTime"/> 类型表示的事件结束日期，此属性在 JSON 序列化时忽略。若结束时间为空，则该属性也为空。
    /// </summary>
    [JsonIgnore]
    public DateTime? EndDate { get; } = End != null ? DateTime.Parse(End) : null;
}

/// <summary>
/// 模拟插件，用于提供当前月和下个月的日历信息。
/// </summary>
/// <remarks>
/// 日历信息进行了简化处理，任何事件都覆盖一整天，并且不针对周末进行特殊处理。
/// </remarks>
internal sealed class CalendarPlugin
{
    /// <summary>
    /// 当前日期
    /// </summary>
    private static readonly DateTime s_now = DateTime.Now;

    /// <summary>
    /// 下个月的第一天
    /// </summary>
    private static readonly DateTime s_nextMonth = new(
        s_now.Year,
        DateTime.Now.AddMonths(1).Month,
        1
    );

    /// <summary>
    /// 存储生成的日历事件列表
    /// </summary>
    private readonly List<CalendarEvent> _events = new();

    /// <summary>
    /// 构造函数，初始化时生成当前月和下个月的日历事件。
    /// </summary>
    public CalendarPlugin()
    {
        CalendarGenerator generator = new();
        this._events =
        [
            .. generator.GenerateEvents(s_now.Month, s_now.Year),
            .. generator.GenerateEvents(s_nextMonth.Month, s_nextMonth.Year),
        ];
    }

    /// <summary>
    /// 用于验证的日历事件只读列表，对插件功能无影响。
    /// </summary>
    public IReadOnlyList<CalendarEvent> Events => this._events;

    /// <summary>
    /// 获取当前日期。
    /// </summary>
    /// <returns>以 "yyyy-MM-dd" 格式表示的当前日期字符串。</returns>
    [KernelFunction]
    public string GetCurrentDate() => DateTime.Now.Date.ToString("yyyy-MM-dd");

    /// <summary>
    /// 获取指定日期范围内开始的已安排事件。
    /// </summary>
    /// <param name="start">日期范围的起始日期</param>
    /// <param name="end">日期范围的结束日期</param>
    /// <returns>符合日期范围的日历事件只读列表。</returns>
    [KernelFunction]
    [Description("获取在指定日期范围内开始的已安排事件。")]
    public IReadOnlyList<CalendarEvent> GetEvents(
        [Description("日期范围的起始日期")] string start,
        [Description("日期范围的结束日期")] string end
    )
    {
        DateTime startDate = DateTime.Parse(start, CultureInfo.CurrentCulture);
        DateTime endDate = DateTime.Parse(end, CultureInfo.CurrentCulture);

        return this
            ._events.Where(e =>
                e.StartDate.Date >= startDate.Date && e.StartDate.Date < endDate.Date.AddDays(1)
            )
            .ToArray();
    }

    /// <summary>
    /// 创建一个新的日程事件。
    /// </summary>
    /// <param name="title">事件标题</param>
    /// <param name="startDate">事件开始日期</param>
    /// <param name="endDate">事件结束日期，可为空</param>
    /// <param name="description">事件描述，可为空</param>
    [KernelFunction]
    [Description("创建一个新的日程事件。")]
    public void NewEvent(
        string title,
        string startDate,
        string? endDate = null,
        string? description = null
    )
    {
        _events.Add(new CalendarEvent(title, startDate, endDate, description));
    }

    /// <summary>
    /// 日历事件生成器的私有类。
    /// </summary>
    private sealed class CalendarGenerator
    {
        /// <summary>
        /// 最大多日事件数量
        /// </summary>
        public int MaximumMultiDayEventCount => this._multiDayEvents.Count;

        /// <summary>
        /// 事件之间的最小间隔天数
        /// </summary>
        public int MinimumEventGapInDays => 3; // 个人日历密度较低

        /// <summary>
        /// 生成指定月份和年份的日历事件。
        /// </summary>
        /// <param name="month">月份</param>
        /// <param name="year">年份</param>
        /// <returns>生成的日历事件的可枚举集合。</returns>
        public IEnumerable<CalendarEvent> GenerateEvents(int month, int year)
        {
            int targetDayOfMonth = 1;
            do
            {
                // 有五分之一的概率生成多日事件，前提是还有多日事件可用
                bool isMultiDay = Random.Shared.Next(5) == 0 && this.MaximumMultiDayEventCount > 0;
                int daySpan = Random.Shared.Next(2, 8);
                int gapDays = Random.Shared.Next(this.MinimumEventGapInDays, 5);

                (string title, string description) = this.Pick(!isMultiDay);

                yield return new CalendarEvent(
                    title,
                    FormatDate(targetDayOfMonth),
                    isMultiDay ? FormatDate(targetDayOfMonth, daySpan) : null,
                    description
                );

                targetDayOfMonth += gapDays + 1 + (isMultiDay ? daySpan : 0);
            } while (targetDayOfMonth <= DateTime.DaysInMonth(year, month));

            string FormatDate(int day, int span = 0)
            {
                DateOnly date = new(year, month, day);
                date = date.AddDays(span);
                return date.ToString("yyyy-MM-dd");
            }
        }

        /// <summary>
        /// 根据是否为单日事件从相应列表中选择一个事件。
        /// </summary>
        /// <param name="isSingleDay">是否为单日事件</param>
        /// <returns>选中事件的标题和描述的元组。</returns>
        private (string title, string description) Pick(bool isSingleDay)
        {
            return Pick(isSingleDay ? _singleDayEvents : _multiDayEvents);
        }

        /// <summary>
        /// 从指定事件列表中随机选择一个事件，并从列表中移除该事件。
        /// </summary>
        /// <param name="eventList">事件列表</param>
        /// <returns>选中事件的标题和描述的元组。</returns>
        private static (string title, string description) Pick(
            List<(string title, string description)> eventList
        )
        {
            int index = Random.Shared.Next(eventList.Count);
            try
            {
                return eventList[index];
            }
            finally
            {
                eventList.RemoveAt(index);
            }
        }

        /// <summary>
        /// 单日事件列表
        /// </summary>
        public readonly List<(string title, string description)> _singleDayEvents =
        [
            ("看医生", "年度体检。"),
            ("杂货店购物", "每周采购生活必需品。"),
            ("瑜伽课", "1 小时的晨练瑜伽课程。"),
            ("汽车保养", "换机油和轮胎换位。"),
            ("与朋友共进晚餐", "在当地餐厅的休闲晚餐。"),
            ("团队会议", "与团队进行项目更新和讨论。"),
            ("理发预约", "在沙龙理发和造型。"),
            ("家长会", "讨论孩子在学校的进展情况。"),
            ("看牙医", "洗牙和常规检查。"),
            ("健身训练", "在健身房进行力量训练。"),
            ("生日派对", "参加朋友的生日庆祝活动。"),
            ("电影之夜", "在电影院观看新上映的电影。"),
            ("志愿者工作", "参加社区清洁活动。"),
            ("求职面试", "应聘潜在新职位的面试。"),
            ("参观图书馆", "归还书籍并浏览新书。"),
        ];

        /// <summary>
        /// 多日事件列表
        /// </summary>
        public readonly List<(string title, string description)> _multiDayEvents =
        [
            ("度假", "与家人的轻松旅行。"),
            ("家庭装修项目", "厨房改造。"),
            ("年度家庭团聚", "前往祖父母家团聚。"),
        ];
    }
}
