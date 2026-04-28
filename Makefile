rwildcard=$(foreach d,$(wildcard $(1:=/*)),$(call rwildcard,$d,$2) $(filter $(subst *,%,$2),$d))

BUILD_DIR = Build
DEPS_DIR = Modules

CMN_DIR = Common
CMN_SRC = $(call rwildcard,$(CMN_DIR),*.cs) $(call rwildcard,$(DEPS_DIR),*.cs)

ASM_DIR = Assembler
ASM_EXE = bisc-asm.exe
ASM_SRC = $(call rwildcard,$(ASM_DIR),*.cs) $(CMN_SRC)

VM_DIR = VirtualMachine
VM_EXE = bisc-vm.exe
VM_SRC = $(call rwildcard,$(VM_DIR),*.cs) $(CMN_SRC)

CSC_FLAGS = -errorendlocation
FILE ?= Programs/console

.PHONY: all
all: $(ASM_EXE) $(VM_EXE) 

.PHONY: help
help:
	@echo "make       : build all make targets"
	@echo "make asm   : run assembler"
	@echo "make vm    : run virtual machine"
	@echo "make test  : run test suite"
	@echo "make docs  : buid documentation"
	@echo "make clean : remove build files"

$(ASM_EXE): $(ASM_SRC)
	@mkdir -p $(BUILD_DIR)
	@csc $(ASM_SRC) -out:$(BUILD_DIR)/$(ASM_EXE) $(CSC_FLAGS)

$(VM_EXE): $(VM_SRC)
	@mkdir -p $(BUILD_DIR)
	@csc $(VM_SRC) -out:$(BUILD_DIR)/$(VM_EXE) $(CSC_FLAGS)

asm: $(ASM_EXE)
	@mono $(BUILD_DIR)/$(ASM_EXE) $(FILE)

vm: $(VM_EXE) $(ASM_EXE)
	@mono $(BUILD_DIR)/$(ASM_EXE) $(FILE)
	@mono $(BUILD_DIR)/$(VM_EXE) $(FILE)

.PHONY: test
test:
	@dotnet test

.PHONY: docs
docs:
	@make pdf -C Modules/BISC-Docs

.PHONY: clean
clean:
	@rm -rf $(BUILD_DIR)
	@rm -rf obj bin
	@rm -rf $(ASM_DIR)/obj $(ASM_DIR)/bin
	@rm -rf $(VM_DIR)/obj $(VM_DIR)/bin
	@rm -rf $(TEST_DIR)/obj $(TEST_DIR)/bin
	@rm -f programs/*.exe
	@rm -f programs/*.bin
