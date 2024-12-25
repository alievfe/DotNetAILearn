from openai import OpenAI

client = OpenAI(
    api_key="",
    base_url="https://internlm-chat.intern-ai.org.cn/puyu/api/v1/",
)

chat_rsp = client.chat.completions.create(
    model="internlm2.5-latest",
    messages=[{"role": "user", "content": "hello"}],
)

for choice in chat_rsp.choices:
    print(choice.message.content)