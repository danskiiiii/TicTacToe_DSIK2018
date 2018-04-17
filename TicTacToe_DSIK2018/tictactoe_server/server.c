/*
    Simple udp server
*/
#include<stdio.h> 
#include<string.h> 
#include<stdlib.h> 
#include<arpa/inet.h>
#include<sys/socket.h>
 
#define BUFLEN 12  
#define PORT 4105   //The port on which to listen for incoming data
 
void die(char *s)
{
    perror(s);
    exit(1);
}
 
int main(void)
{
    struct sockaddr_in myaddr, endpointOne, endpointTwo, endpGame;
 
    int s, i, num=48, slenOne = sizeof(endpointOne),slenTwo = sizeof(endpointTwo) ,slenGame = sizeof(endpGame), recv_len;
    char buf[BUFLEN];
	memset(buf,0,sizeof(buf));	 

 
    //create a UDP socket
    if ((s=socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP)) == -1)
    {
        die("socket");
    }
 
    // zero out the structure
    memset((char *) &myaddr, 0, sizeof(myaddr));
 
    myaddr.sin_family = AF_INET;
    myaddr.sin_port = htons(PORT);
    myaddr.sin_addr.s_addr = htonl(INADDR_ANY);
 
    //bind socket to port
    if( bind(s , (struct sockaddr*)&myaddr, sizeof(myaddr) ) == -1)
    {
        die("bind");
    }
 
    //keep listening for data
    while(1)
    {
        printf("Waiting for 1st player...");
        fflush(stdout); 
        //try to receive some data, this is a blocking call
        if ((recv_len = recvfrom(s, buf, BUFLEN, 0, (struct sockaddr *) &endpointOne, &slenOne)) == -1)
        {
            die("recvfrom()");
        }  
        
 
        //print details of the client/peer and the data received
        printf("Received packet from %s:%d\n", inet_ntoa(endpointOne.sin_addr), ntohs(endpointOne.sin_port));
        printf("Data: %s\n" , buf);
		
		
		
		printf("Waiting for 2nd player...");
		fflush(stdout); 
		//try to receive some data, this is a blocking call
        if ((recv_len = recvfrom(s, buf, BUFLEN, 0, (struct sockaddr *) &endpointTwo, &slenTwo)) == -1)
        {
            die("recvfrom()");
        } 
 
        //print details of the client/peer and the data received
        printf("Received packet from %s:%d\n", inet_ntoa(endpointTwo.sin_addr), ntohs(endpointTwo.sin_port));
        printf("Data: %s\n" , buf);
		
		 //now reply the client to determine turn
		        memset(buf,0,sizeof(buf));	
                buf[0]='O';
        if (sendto(s, buf, recv_len, 0, (struct sockaddr*) &endpointOne, slenOne) == -1)
        {
            die("sendto()");
        }
                memset(buf,0,sizeof(buf));	
                buf[0]='X';
       if (sendto(s, buf, recv_len, 0, (struct sockaddr*) &endpointTwo, slenTwo) == -1)
        {
            die("sendto()");
        }
       memset(buf,0,sizeof(buf));		
		
		
	//	if (!strcmp(buf,"Hello? Button4O"))
	//{ printf("IF SIE SPEŁNIŁ");}      


while (1){
	//try to receive some data, this is a blocking call
        if ((recv_len = recvfrom(s, buf, BUFLEN, 0, (struct sockaddr *) &endpGame, &slenGame)) == -1)
        {
            die("recvfrom()");
        }  
        //print details of the client/peer and the data received
        printf("Received packet from %s:%d\n", inet_ntoa(endpGame.sin_addr), ntohs(endpGame.sin_port));
        printf("Data: %s\n" , buf);
		
	//exit loop and find new game	
	if (!strcmp(buf,"ready"))
		{break;}
	
	//send file	
	if (!strcmp(buf,"getfile"))
		{
		 memset(buf,0,sizeof(buf));	

			char file_buf[65001];
			memset(file_buf,0,sizeof(file_buf));

		char buftitle[11]="myfile0.jpg";
		buftitle[6]=num;		
		num++;
		if (num==58) {num=48;}		

        FILE *fp = fopen(buftitle, "rb");
        if (fp != NULL) {
        size_t file_size = fread(file_buf, sizeof(char), 65000, fp);
        if (file_size == 0) {fputs("Error reading file", stderr); } 
	    else {file_buf[++file_size] = '\0'; /* Just to be safe. */
             }

         fclose(fp);
		
			if (sendto(s, file_buf, file_size, 0, (struct sockaddr*) &endpointOne, slenOne) == -1)
        {
            die("sendto()");
        }
			continue;
		}
		
		}
	 //now reply the client with the same data
        if (sendto(s, buf, recv_len, 0, (struct sockaddr*) &endpointOne, slenOne) == -1)
        {
            die("sendto()");
        }

       if (sendto(s, buf, recv_len, 0, (struct sockaddr*) &endpointTwo, slenTwo) == -1)
        {
            die("sendto()");
        }
       memset(buf,0,sizeof(buf));	
	
	
	
}

 

 } 
 
    close(s);
    return 0;
}