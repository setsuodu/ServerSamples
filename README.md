# ServerSamples

## 方案

- Host on Windows
- Host on Ubuntu
- Host on Docker
- Host on Kubernetes

> 第一类：平台解决方案
- Unity Game Services / Multiplay
	- cons: price
- Google Agones
	- pros: open source, scaling, globally, Kubernetes
	- cons: Kubernetes, learning cost
	- 费用在于养具有“容器化技术经验”的工程师（成为Edgegap等“开盒即用”派的攻击点）
- Google Open Match
	- https://github.com/googleforgames/open-match
	- an open-source game matchmaking framework
	- pros: Extensibility, Flexibility, Scalability
	- cons: write by GO, Kubernetes, learning cost
- Microsoft Playfab
- Amazon Gamelift
	- cons: price
- Nakama
	- pros: dashboard, easy API, Docker
	- cons: single machine is free, pay to scaling
- Beamable
	- An Open Game Server Ecosystem,
	- pros: Microservices, Dashboard
	- cons: price(Free to develop up to 100k API calls), 限制微服务数量为3
- Edgegap
	- Edgegap is on a mission to make distributed infrastructures easy to use.
	- pros: easy deploy, Docker, distributed
	- cons: pay-as-you-use hosting
- Normcore
	- cons: price
- Photon / PUN
	- cons: price
- GameSpark
	- cons: price
- Steamworks
	- cons: limitations

> 第二类：手写简易连接
- FishNet-EasyServerList
- PHP-ServerList

> 第三类：其他开源框架
- Orleans(.net)
	- pros: Actor, dashboard
	- cons: learning cost
- ET(.net)
	- pros: Actor, ECS
	- cons: learning cost
- Master Server Kit(unity)
	- pros: instance architect, dashboard
	- cons: port connect bug
- Noble Whale - Match Up(c++ lib)
- pomelo(node.js)
- Hathora(next.js)

## CI(持续集成)/CD(持续部署) DevOps

- 敏捷(Scrum)、协同(Coop)、容器(Container)、微服务(Serverless), etc, 诸多时下最流行的开发模式。
- Actions, Workflows, Packages
- Jenkins
- Github Actions
	- Jenkins and GitHub Actions both allow you to create workflows 
	that automatically build, test, publish, release, and deploy code.
	- [ ] 自动部署到腾讯云
	- [ ] https://youtu.be/-txXtAfViEQ

## 架构

- Interface搭建可更换传输层的网络架构。如编写Android/iOS插件的架构。
- OnConnected += Action
- OnDisconnected += Action
- OnReceive += Action
- Send += Action
