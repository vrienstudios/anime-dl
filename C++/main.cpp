#include <iostream>
#include <string>
#include <cstddef>

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

	return 0;
}
