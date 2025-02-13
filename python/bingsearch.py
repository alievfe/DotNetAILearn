# 导入 argparse 模块，用于解析命令行参数
import argparse
# 导入 logging 模块，用于记录程序运行时的日志信息
import logging
# 导入 uuid 模块，用于生成通用唯一识别码
import uuid
# 从 urllib.parse 模块导入 urlencode 函数，用于将字典形式的参数编码为 URL 查询字符串
from urllib.parse import urlencode

# 导入 requests 库，用于发送 HTTP 请求（在当前代码中未使用，但可能后续扩展会用到）
import requests
# 从 bs4 库导入 BeautifulSoup 类，用于解析 HTML 文档
from bs4 import BeautifulSoup
# 从 playwright.sync_api 模块导入 sync_playwright 函数，用于同步执行 Playwright 自动化操作
from playwright.sync_api import sync_playwright

# 定义摘要的最大长度为 500 个字符
ABSTRACT_MAX_LENGTH = 500

# 配置日志系统，设置日志级别为 INFO，即记录重要的运行信息
logging.basicConfig(level=logging.INFO)
# 创建一个名为当前模块名的日志记录器
logger = logging.getLogger(__name__)

# 定义 Bing 搜索的基础 URL，后续会在该 URL 后添加查询参数
BING_SEARCH_URL = "https://cn.bing.com/search?"
# 定义 Bing 的主机 URL
BING_HOST_URL = "https://www.bing.com"


# 定义一个名为 BingSearch 的类，用于封装 Bing 搜索相关的操作
class BingSearch:
    def __init__(self):
        # 启动 Playwright 浏览器自动化工具
        self.p = sync_playwright().start()
        # 使用 Chromium 浏览器引擎启动一个浏览器实例
        self.browser = self.p.chromium.launch()

    def __enter__(self):
        # 实现上下文管理器的 __enter__ 方法，返回当前对象实例
        return self

    def search(self, keyword, num_results=30, debug=0):
        """
        通过 Playwright 进行多页面检索，因 Playwright 完全模拟浏览器，加载了更多文件，所以速度比较慢。
        :param keyword: 关键字
        :param num_results: 指定返回的结果个数，支持多页检索，返回数量超过 10 个结果
        :param debug: 是否启用调试模式
        :return: 结果列表
        """
        # 如果关键字为空，直接返回空列表
        if not keyword:
            return []

        # 如果指定的结果数量小于等于 0，直接返回空列表
        if num_results <= 0:
            return []

        # 初始化一个空列表，用于存储搜索结果
        list_result = []

        # 在浏览器中创建一个新的页面
        page = self.browser.new_page()

        # 当已获取的结果数量小于指定的结果数量时，继续进行搜索
        while len(list_result) < num_results:
            try:
                # 构建搜索 URL 的参数
                params = {
                    # 搜索关键字
                    "q": keyword,
                    # 生成一个唯一的标识符作为 FPIG 参数
                    "FPIG": str(uuid.uuid4()).replace('-', ''),
                    # 从第几个结果开始显示
                    "first": len(list_result),
                    # 搜索表单类型
                    "FORM": "PORE"
                }
                # 将参数编码为 URL 查询字符串，并拼接在基础搜索 URL 后面
                next_url = BING_SEARCH_URL + urlencode(params)
                # 让浏览器页面跳转到构建好的搜索 URL
                page.goto(url=next_url)

                # 以下代码被注释掉，功能是截图保存到唯一文件名
                screenshot_path = f'example_{uuid.uuid4()}.png'
                page.screenshot(path=screenshot_path)

                # 获取当前页面的 HTML 内容
                res_text = page.content()
                # 使用 BeautifulSoup 解析 HTML 内容
                root = BeautifulSoup(res_text, "lxml")

                # 在 HTML 中查找 id 为 "b_content" 的元素，再查找其下的 main 元素，最后查找 id 为 "b_results" 的元素
                ol = root.find(id="b_content").find(
                    "main").find(id="b_results")
                # 如果没有找到搜索结果列表，记录警告信息并跳出循环
                if not ol:
                    logger.warning("No search results found.")
                    break

                # 遍历搜索结果列表中的每个元素
                for li in ol.contents:
                    # 获取当前元素的类名列表，如果没有类名则返回空列表
                    classes = li.get("class", [])
                    # 如果当前元素的类名包含 "b_pag"，表示这是分页元素
                    if "b_pag" in classes:
                        try:
                            # 点击下一页按钮
                            page.locator(
                                "#b_results > li.b_pag > nav > ul > li > a.sb_pagN").click()
                        except Exception as e:
                            # 如果点击下一页按钮失败，记录错误信息并跳出循环
                            logger.error(
                                f"Failed to click next page button: {e}")
                            break

                    # 如果当前元素的类名不包含 "b_algo"，则跳过该元素
                    if "b_algo" not in classes:
                        continue

                    # 提取搜索结果的标题
                    news_title = li.find("h2").find("a").get_text(strip=True)
                    # 提取搜索结果的链接
                    news_url = li.find("div", class_="b_tpcn").find(
                        "a").get("href", "")
                    # 提取搜索结果的摘要，并截取前 ABSTRACT_MAX_LENGTH 个字符
                    news_abstract = li.find("div", class_="b_caption").get_text(
                        strip=True)[:ABSTRACT_MAX_LENGTH]
                    # 将搜索结果信息添加到结果列表中
                    list_result.append({
                        # 结果的排名
                        "rank": len(list_result) + 1,
                        # 结果的标题
                        "title": news_title,
                        # 结果的链接
                        "url": news_url,
                        # 结果的摘要
                        "abstract": news_abstract
                    })

            except Exception as e:
                # 如果在解析页面 HTML 时出现异常，且处于调试模式，则记录错误信息
                if debug:
                    logger.error(f"Exception during parsing page HTML: {e}")
                # 跳出循环
                break

        # 返回指定数量的搜索结果
        return list_result[:num_results]

    def release_resource(self):
        try:
            # 关闭浏览器实例
            self.browser.close()
            # 停止 Playwright 自动化工具
            self.p.stop()
        except Exception as e:
            # 如果释放资源时出现异常，忽略该异常
            # print(f"Exception: {e}")
            pass

    def __del__(self):
        # 当对象被销毁时，调用 release_resource 方法释放资源
        self.release_resource()

    def __exit__(self, exc_type, exc_val, exc_tb):
        # 实现上下文管理器的 __exit__ 方法，在退出上下文时调用 release_resource 方法释放资源
        self.release_resource()


def run():
    """
    主程序入口，支持命令行带参执行或者手动输入关键字
    :return:
    """

    # 创建一个 ArgumentParser 对象，用于解析命令行参数
    parser = argparse.ArgumentParser(
        description='Bing search engine.')

    # 添加一个位置参数，用于接收搜索关键字
    parser.add_argument('keyword', type=str)
    # 添加一个可选参数 -n 或 --num_results，用于指定返回的结果数量，默认值为 10
    parser.add_argument('-n', '--num_results', type=int,
                        default=10)
    # 添加一个可选参数 -d 或 --debug，用于启用调试模式
    parser.add_argument('-d', '--debug',
                        action='store_true')

    # 解析命令行参数
    args = parser.parse_args()

    # 使用上下文管理器创建 BingSearch 对象实例
    with BingSearch() as bs:
        # 调用 search 方法进行搜索
        results = bs.search(
            args.keyword, num_results=args.num_results, debug=args.debug)

    # 如果搜索结果是一个列表
    if isinstance(results, list):
        # 打印搜索结果的总数
        print("search results：(total[{}]items.)".format(len(results)))
        # 遍历搜索结果列表
        for res in results:
            # 打印每个搜索结果的排名、标题、摘要和链接
            print("{}. {}\n   {}\n   {}".format(
                res['rank'], res["title"], res["abstract"], res["url"]))
    else:
        # 如果搜索结果不是列表，打印搜索失败信息
        print("start search: [{}] failed.".format(args.keyword))


# if __name__ == '__main__':
#     run()

# 创建 BingSearch 对象实例
bs = BingSearch()
# 调用 search 方法搜索关键字 "python"
data = bs.search("python")
# 打印搜索结果
print(data)