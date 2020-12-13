#include <iostream>
#include <string>
#include <array>
#include <assert.h>
#include "main.h"

#ifdef _WIN32
	#include <stdio.h>
	#include <stdlib.h>
    #define popen _popen
    #define pclose _pclose
#else
	#include <cstdlib>
	#include <cstdio>
	#include <cstddef>
#endif

int main(int argc, char *argv[]){

	std::cout << "Running anime-dl" << std::endl;

	if(argc <= 1){
		std::cout << "HELP: \n     -d | Download flag\n     -c | Skip flag\n     -S | Search flag\n";
		return 1;
	}

 	unsigned char download;
	unsigned char skip;
	unsigned char search;
	
	std::string searchString = "";
	--argc;

	for(int idx = 0; idx < argc; idx++){
		std::string line = std::string(argv[idx]);
		if(line == "-d")
			download = 1;
		else if(line == "-S"){
			search = 1;
			do
			{
				++idx;
				line = std::string(argv[idx]);
				searchString += line;
				searchString += " ";
			}
			while(line[0] != '-' && idx < argc);
		}
		else if(line == "-c")
			skip = 1;
	}

	if(search == 1){
		std::cout << "Starting Search!\n";
		getSearchPage(searchString);
		std::cout << "\nSearching for Video\n";
	}
	
	return 0;
}

std::string getSearchPage(std::string searchString){
		std::string result;
		char buf[1028];
		snprintf(buf, sizeof(buf), "wget https://gogo-stream.com/search.html?keyword=%s --no-check-certificate -np -q -O -", searchString.c_str());
		std::array<char, 128> buffer;
		FILE* pipe = popen(buf, "r");
		if (!pipe)
		{
			std::cout << "Couldn't start command." << std::endl;
			return 0;
		}
		while (fgets(buffer.data(), 128, pipe) != NULL) {
			result += buffer.data();
		}
		int returnCode = pclose(pipe);
		std::cout << "Return code:" << returnCode << ((returnCode == 4) ? "\nSuccess!" : "\nFailed attempt?") << std::endl;
		
		return result;
}

std::array<std::string, 400> getVideoList(std::string searchPageHTML){
	std::array<std::string, 400> list;
	return list;
}