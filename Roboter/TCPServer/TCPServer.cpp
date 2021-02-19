#include "TCPServer.h"

#define buffer_size 1024
#define PORT 54000

std::vector<StringData> multiArgs;
bool multicommands = false;

int multiindex = 0;

bool ready = true;

void TCPServer(bool &lidarStart, bool &serialstart)
{

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
    if (setsockopt(master_socket, SOL_SOCKET, SO_REUSEADDR, (char *)&opt, sizeof(opt)) < 0)
    {
        perror("setsockopt");
        exit(EXIT_FAILURE);
    }

    //type of socket created
    address.sin_family = AF_INET;
    address.sin_addr.s_addr = INADDR_ANY;
    address.sin_port = htons(PORT);

    //bind the socket to localhost port 54001
    if (bind(master_socket, (struct sockaddr *)&address, sizeof(address)) < 0)
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
            if ((new_socket = accept(master_socket, (struct sockaddr *)&address, (socklen_t *)&addrlen)) < 0)
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
                    getpeername(sd, (struct sockaddr *)&address, (socklen_t *)&addrlen);
                    printf("Client disconnected , ip %s , port %d \n", inet_ntoa(address.sin_addr), ntohs(address.sin_port));

                    //Close the socket and mark as 0 in list for reuse
                    close(sd);
                    client_socket[i] = 0;
                }
                else
                {
                    std::string data = std::string(buffer);
                    memset(buffer, 0, buffer_size);
                    int found = data.find(' ');

                    if (found != 1)
                    {
                        std::vector<std::string> comands;
                        std::string comand;
                        std::stringstream ss(data);
                        while (getline(ss, comand, ' '))
                        {
                            comands.push_back(comand);
                        }

                        if (comands[0] == "lidar")
                        {
                            if (lidarStart)
                            {
                                if (comands[1] == "sumdata")
                                {

                                    std::map<int, int> lidardata;

                                    int howmany = std::stoi(comands[2]);
                                    calculateaverage(lidardata, howmany);

                                    int maxargs = 50;
                                    std::vector<std::string> strings;
                                    std::ostringstream sstring;

                                    sstring << "lidarmap "
                                            << "data ";

                                    int maxvalues = 0;
                                    for (auto &i : lidardata)
                                    {
                                        if (maxvalues > maxargs)
                                        {
                                            std::string output = sstring.str();
                                            strings.push_back(output);
                                            output.clear();
                                            sstring.str("");
                                            sstring << "lidarmap "
                                                    << "data ";
                                            maxvalues = 0;
                                        }
                                        sstring << i.first << ";" << i.second << ",";
                                        maxvalues++;
                                    }
                                    std::string output = sstring.str();
                                    strings.push_back(output);
                                    for (auto &i : strings)
                                    {
                                        std::this_thread::sleep_for(std::chrono::milliseconds(100)); // Wait 100ms to send data;
                                        int bytecount = send(sd, i.c_str(), i.size() + 1, 0);
                                        i.clear();
                                    }
                                    std::ostringstream ssend;
                                    ssend << "lidarmap "
                                          << "end ";
                                    std::string end = ssend.str();
                                    std::this_thread::sleep_for(std::chrono::milliseconds(100));
                                    send(sd, end.c_str(), end.size() + 1, 0);
                                }
                            }
                            else
                            {
                                std::string error = "ERROR Lidar_Could_not_start!";
                                send(sd, error.c_str(), error.size() + 1, 0);
                            }
                        }

                        if (comands[0] == "roboter")
                        {
                            if (serialstart == false)
                            {

                                if (comands[1] == "move")
                                {

                                    if (comands.size() == 3)
                                    {
                                        std::vector<std::string> args;
                                        std::stringstream sstring(comands[2]);
                                        std::string out;
                                        while (getline(sstring, out, ','))
                                        {
                                            args.push_back(out);
                                        }
                                        float x = std::stoi(args[0]);
                                        float y = std::stoi(args[1]);
                                        float speed = std::stoi(args[2]);
                                        multicommands = false;
                                        multiindex = 0;
                                        multiArgs.clear();
                                        moveRobot(x, y, speed);
                                    }
                                }
                                else if (comands[1] == "rotate")
                                {
                                    if (comands.size() == 3)
                                    {
                                        std::vector<std::string> args;
                                        std::stringstream sstring(comands[2]);
                                        std::string out;
                                        while (std::getline(sstring, out, ','))
                                        {
                                            args.push_back(out);
                                        }
                                        float a = std::stoi(args[0]);
                                        float speed = std::stoi(args[0]);
                                        multicommands = false;
                                        multiindex = 0;
                                        multiArgs.clear();
                                        rotateRobot(a, speed);
                                    }
                                    else
                                    {
                                        std::string error = "ERROR ToMatchArgs";
                                        send(sd, error.c_str(), error.size() + 1, 0);
                                    }
                                }
                                else if (comands[1] == "multi")
                                {

                                    std::vector<StringData> args;
                                    std::stringstream sstring(comands[2]);
                                    std::string out;
                                    std::string out2;
                                    while (std::getline(sstring, out, ','))
                                    {
                                        std::getline(sstring, out2, ',');

                                        args.push_back({out, out2});
                                    }
                                    std::cout << "test" << std::endl;
                                    multiArgs = args;

                                    multicommands = true;
                                }
                                else if (comands[1] == "stop")
                                {
                                    multicommands = false;
                                    multiindex = 0;
                                    multiArgs.clear();
                                    moveRobot(0, 0, 0);
                                }
                            }
                            else
                            {
                                std::string error = "ERROR SerialPort_Not_Open";
                                send(sd, error.c_str(), error.size() + 1, 0);
                            }
                        }

                        if (comands[0] == "tcp")
                        {
                            if (comands[1] == "test")
                            {
                                ready = true;
                            }
                            if (comands[1] == "reboot")
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
        if (multicommands)
        {
            ready = roboterreadData();
            if (ready)
            {
                ready = false;

                if (multiindex < multiArgs.size())
                {
                    if (multiArgs[multiindex].command == "move")
                    {
                        std::cout << multiindex << "|" << multiArgs.size() << std::endl;
                        std::cout << multiArgs[multiindex].args << std::endl;
                        std::stringstream sstring(multiArgs[multiindex].args);
                        std::string out;
                        std::vector<int> numbers;

                        while (std::getline(sstring, out, ';'))
                        {
                            numbers.push_back(std::stoi(out));
                        }
                        std::cout << numbers[0] << "|" << numbers[1] << "|" << numbers[2] << std::endl;
                        moveRobot(numbers[0],numbers[1],numbers[2]);

                        //moveRobot(, numbers[1], numbers[2]);
                    }else if(multiArgs[multiindex].command == "rotate"){
                        std::cout << multiindex << "|" << multiArgs.size() << std::endl;
                        std::cout << multiArgs[multiindex].args << std::endl;
                        std::stringstream sstring(multiArgs[multiindex].args);
                        std::string out;
                        std::vector<int> numbers;

                        while (std::getline(sstring, out, ';'))
                        {
                            numbers.push_back(std::stoi(out));
                        }
                        std::cout << numbers[0] << "|" << numbers[1] << std::endl;
                        rotateRobot(numbers[0],numbers[1]);
                    }
                    multiindex++;
                }
                else
                {
                    multicommands = false;
                    multiindex = 0;
                }
            }
        }
    }

    close(master_socket);

    if (reboot)
    {
        system("./test");
    }

    return;
}