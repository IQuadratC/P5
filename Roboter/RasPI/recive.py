import socket

# take the server name and port name
host = 'local host'
port = 5000

# create a socket at server side using TCP / IP protocol
s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

# bind the socket with server and port number
s.bind(('', port))

# allow maximum 1 connection to the socket
s.listen(1)

while True:
    # wait till a client accept connection
    c, addr = s.accept()

    # display client address
    print("CONNECTION FROM:", str(addr))

    # receive message string from server, at a time 1024 B
    msg = c.recv(1024)
    print(msg.decode("utf-8").split(" ", 2)[1])

    # disconnect the server
    c.close()
