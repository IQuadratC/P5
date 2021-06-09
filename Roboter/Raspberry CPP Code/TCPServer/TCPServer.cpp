#include "TCPServer.h"

#define buffer_size 1024
#define PORT 54000

static int maxSendSize = 50;
static int multiindex = 0;
static bool ready = false;
static bool flagStop = false;

static std::future<void> multiFunktion;

static std::map<int, ClientData> clientdata;

static std::set<Vector2> MainMapPointsSend;

extern std::map<Vector2, PointsFound> MainMapPoints;

extern Vector2 roboterPos;
extern int roboterAngle;

void multiCommandsFunktion(std::vector<StringData>& multiArgs) {

	for (;;)
	{
		if (flagStop) {
			flagStop = false;
			return;
		}
		if (ready) {

			if (multiindex < multiArgs.size())
			{
				if (multiArgs[multiindex].command == "move")
				{
					std::cout << multiindex << "|" << multiArgs.size() << std::endl;
					std::cout << multiArgs[multiindex].args << std::endl;
					std::stringstream sstring(multiArgs[multiindex].args);
					std::string out;
					std::vector<int> numbers;

					while (std::getline(sstring, out, ';')) numbers.push_back(std::stoi(out));

					std::cout << numbers[0] << "|" << numbers[1] << "|" << numbers[2] << std::endl;
					moveRobot(numbers[0], numbers[1], numbers[2]);
				}
				else if (multiArgs[multiindex].command == "rotate")
				{
					std::cout << multiindex << "|" << multiArgs.size() << std::endl;
					std::cout << multiArgs[multiindex].args << std::endl;
					std::stringstream sstring(multiArgs[multiindex].args);
					std::string out;
					std::vector<int> numbers;

					while (std::getline(sstring, out, ';')) numbers.push_back(std::stoi(out));

					std::cout << numbers[0] << "|" << numbers[1] << std::endl;
					rotateRobot(numbers[0], numbers[1]);
				}
				multiindex++;
			}
			else
			{
				multiArgs.clear();
				return;
			}
		}
		ready = roboterReadData();
	}
}

//TCP Server Quelle: https://gist.github.com/rajabishek/7102fe2b7b60b4833d72
void TCPServer(bool& lidarstart)
{

	bool serialstart;
	startserial(serialstart);

	bool reboot = false;

	int opt = 1;
	int master_socket, addrlen, new_socket, client_socket[30], max_clients = 30, activity, i, valread, sd;
	int max_sd;
	struct sockaddr_in address;

	//data buffer of 1K
	char buffer[buffer_size];

	memset(buffer, 0, buffer_size);

	//set of socket descriptors
	fd_set readfds;

	//a message
	std::string message = "Connected!\r\n";

	//initialise all client_socket[] to 0 so not checked
	for (i = 0; i < max_clients; i++)
	{
		client_socket[i] = 0;
	}

	//create a master socket
	if ((master_socket = socket(AF_INET, SOCK_STREAM, 0)) == 0)
	{
		perror("socket failed");
		exit(EXIT_FAILURE);
	}

	//set master socket to allow multiple connections , this is just a good habit, it will work without this
	if (setsockopt(master_socket, SOL_SOCKET, SO_REUSEADDR, (char*)&opt, sizeof(opt)) < 0)
	{
		perror("setsockopt");
		exit(EXIT_FAILURE);
	}

	//type of socket created
	address.sin_family = AF_INET;
	address.sin_addr.s_addr = INADDR_ANY;
	address.sin_port = htons(PORT);

	//bind the socket to localhost port 54001
	if (bind(master_socket, (struct sockaddr*)&address, sizeof(address)) < 0)
	{
		perror("bind failed");
		exit(EXIT_FAILURE);
	}
	printf("Listener on port %d \n", PORT);

	//try to specify maximum of 3 pending connections for the master socket
	if (listen(master_socket, 3) < 0)
	{
		perror("listen");
		exit(EXIT_FAILURE);
	}

	//accept the incoming connection
	addrlen = sizeof(address);
	puts("Waiting for connections ...");

	bool running = true;

	while (running)
	{
		try {

			//clear the socket set
			FD_ZERO(&readfds);

			//add master socket to set
			FD_SET(master_socket, &readfds);
			max_sd = master_socket;

			//add child sockets to set
			for (i = 0; i < max_clients; i++)
			{
				//socket descriptor
				sd = client_socket[i];

				//if valid socket descriptor then add to read list
				if (sd > 0)
					FD_SET(sd, &readfds);

				//highest file descriptor number, need it for the select function
				if (sd > max_sd)
					max_sd = sd;
			}

			//wait for an activity on one of the sockets , timeout is NULL , so wait indefinitely
			activity = select(max_sd + 1, &readfds, NULL, NULL, NULL);

			if ((activity < 0) && (errno != EINTR))
			{
				printf("select error");
			}

			//If something happened on the master socket , then its an incoming connection
			if (FD_ISSET(master_socket, &readfds))
			{
				if ((new_socket = accept(master_socket, (struct sockaddr*)&address, (socklen_t*)&addrlen)) < 0)
				{
					perror("accept");
					exit(EXIT_FAILURE);
				}

				//inform user of socket number - used in send and receive commands
				printf("New connection , socket fd is %d , ip is : %s , port : %d \n", new_socket, inet_ntoa(address.sin_addr), ntohs(address.sin_port));

				//send new connection greeting message
				if (send(new_socket, message.c_str(), message.size(), 0) != message.size())
				{
					perror("send");
				}

				puts("Welcome message sent successfully");

				//add new socket to array of sockets
				for (i = 0; i < max_clients; i++)
				{
					//if position is empty
					if (client_socket[i] == 0)
					{
						client_socket[i] = new_socket;
						clientdata[i].ip = inet_ntoa(address.sin_addr);
						clientdata[i].permission = 0;
						printf("Adding to list of sockets as %d\n", i);

						break;
					}
				}
			}

			//else its some IO operation on some other socket :)
			for (i = 0; i < max_clients; i++)
			{
				sd = client_socket[i];

				if (FD_ISSET(sd, &readfds))
				{
					//Check if it was for closing , and also read the incoming message
					if ((valread = read(sd, buffer, buffer_size)) == 0)
					{
						//Somebody disconnected , get his details and print
						getpeername(sd, (struct sockaddr*)&address, (socklen_t*)&addrlen);
						printf("Client disconnected , ip %s , port %d \n", inet_ntoa(address.sin_addr), ntohs(address.sin_port));

						//Close the socket and mark as 0 in list for reuse
						close(sd);
						client_socket[i] = 0;
						clientdata.erase(clientdata.find(i));
						MainMapPointsSend.clear();
					}
					else
					{
						std::string data = std::string(buffer);
						memset(buffer, 0, buffer_size);
						int found = data.find(' ');

						if (found != 1)
						{
							std::vector<std::string> commands;
							std::string command;
							std::stringstream ss(data);

							while (getline(ss, command, ' ')) commands.push_back(command);

							if (commands[0] == "lidar")
							{
								if (lidarstart)
								{
									if (commands[1] == "mapdata") {
										moveRobot(0, 0, 0);
										std::cout << "MapData" << std::endl;
										std::vector<std::string> strings;
										std::ostringstream sstring;

										sstring << "lidarmap " << "data ";

										int maxvalues = 0;
										for (auto i : MainMapPoints)
										{
											if (MainMapPointsSend.find({ i.first.x ,i.first.y }) != MainMapPointsSend.end()) continue;

											if (maxvalues > maxSendSize)
											{
												std::string output = sstring.str();
												strings.push_back(output);
												output.clear();
												sstring.str("");
												sstring << "lidarmap " << "data ";
												maxvalues = 0;
											}
											if (i.second.counter > 0) {
												sstring << i.first.x << ";" << i.first.y << ",";
												MainMapPointsSend.insert({ i.first.x,i.first.y });
												maxvalues++;
											}

										}
										std::string output = sstring.str();
										strings.push_back(output);
										for (auto& i : strings)
										{
											std::this_thread::sleep_for(std::chrono::milliseconds(delay)); // Wait 100ms to send data;
											i = i.substr(0, i.size() - 1);
											int bytecount = send(sd, i.c_str(), i.size() + 1, 0);
											i.clear();
										}
										std::ostringstream ssend;
										ssend << "lidarmap " << "end ";
										std::string end = ssend.str();
										std::this_thread::sleep_for(std::chrono::milliseconds(delay));
										send(sd, end.c_str(), end.size() + 1, 0);
									}
								}
								else
								{
									std::string error = "ERROR Lidar_Could_not_start!";
									send(sd, error.c_str(), error.size() + 1, 0);
								}
							}

							if (commands[0] == "roboter")
							{
								if (serialstart)
								{
									if (commands[1] == "move")
									{
										if (commands.size() == 3)
										{
											std::vector<std::string> args;
											std::stringstream sstring(commands[2]);
											std::string out;
											while (getline(sstring, out, ','))
											{
												args.push_back(out);
											}
											int x = std::stoi(args[0]);
											int y = std::stoi(args[1]);
											int speed = std::stoi(args[2]);

											moveRobot(x, y, speed);

										}
									}
									else if (commands[1] == "rotate")
									{
										if (commands.size() == 3)
										{
											std::vector<std::string> args;
											std::stringstream sstring(commands[2]);
											std::string out;
											while (std::getline(sstring, out, ','))
											{
												args.push_back(out);
											}
											int a = std::stoi(args[0]);
											int speed = std::stoi(args[1]);

											rotateRobot(a, speed);

										}
										else
										{
											std::string error = "ERROR Not_enough_args";
											send(sd, error.c_str(), error.size() + 1, 0);
										}
									}
									else if (commands[1] == "multi")
									{
										if (commands.size() > 2)
										{
											multiFunktion.get();
											std::vector<StringData> args;
											std::stringstream sstring(commands[2]);
											std::string out;
											std::string out2;
											while (std::getline(sstring, out, ','))
											{
												std::getline(sstring, out2, ',');

												args.push_back({ out, out2 });
											}

											multiFunktion = std::async(multiCommandsFunktion, std::ref(args));
										}

									}
									else if (commands[1] == "stop")
									{
										moveRobot(0, 0, 0);
									}
									else if (commands[1] == "position") {
										std::cout << "Position" << std::endl;
										std::stringstream sstring;
										std::string data;
										sstring << "roboter position ";
										sstring << roboterPos.x << "," << roboterPos.y << "," << roboterAngle;

										data = sstring.str();
										send(sd, data.c_str(), data.size() + 1, 0);
									}
								}
								else
								{
									std::string error = "ERROR SerialPort_Not_Open";
									send(sd, error.c_str(), error.size() + 1, 0);
								}
							}

							if (commands[0] == "tcp")
							{

								if (commands[1] == "reboot")
								{
									running = false;
									reboot = true;
									break;
								}
							}

						}
						else
						{
							std::string error = "ERROR ERROR";
							send(sd, error.c_str(), error.size() + 1, 0);
						}
					}
				}
			}
		}
		catch (const std::exception& ex) {
			std::cout << "Error" << std::endl;
		}
	}

	close(master_socket);

	if (reboot)
	{
		system("clear");
		system("./test");
		exit(-2);
		
	}

	return;
}
