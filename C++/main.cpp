#include <iostream>
#include <string>
#include <array>
#include <assert.h>
#include <cstring>
#include <regex>
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
	#include <errno.h>
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
		searchString = getSearchPage(searchString);
		std::cout << "\nSearching for Video\n";
		std::vector<std::string> koo = getVideoLink(searchString);
		std::cout << koo[0] << std::endl;
		std::cout << "Getting video list" << std::endl;
		std::string alko = koo[1];
		alko[0] = toupper(alko[0]);
		std::cout << alko << std::endl;
		//std::cout << "Full link: " << searchString << std::endl;
	}

	
	return 0;
}

std::string getSearchPage(std::string searchString){
		std::string result;
		int lilnux = 0;
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
		std::string er = strerror(errno);
		std::cout << "Return code:" << returnCode << "\nFlag: " << er;
		
		if(lilnux == 1 && returnCode != 0 || returnCode != 4)
			std::cout << "\n\n" << er;
		
		return result;
}

std::vector<std::string> getVideoLink(std::string searchPageHTML){
	std::smatch match;
	std::regex r("<a href=\"(/videos/(.*?))\"");

	std::regex_search(searchPageHTML, match, r);
	std::cout << "Got Video: " << match[1] << std::endl;
	std::cout << "2 : " << match[0].str() << std::endl;
	std::string ak = "https://gogo-stream.com";
	ak += match[1];

	std::vector<std::string> obj;
	obj.push_back(ak);
	std::regex n("https://gogo-stream.com/videos/(.*?)-episode");

	std::smatch nmatch;
	std::regex_search(ak, nmatch, n);
	obj.push_back(nmatch[1].str());

	ak.clear();
	return obj;
}

std::vector<std::string> getVideoLinks(std::string videoA){
	std::vector<std::string> list;



	return list;
}
