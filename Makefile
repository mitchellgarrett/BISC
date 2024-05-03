rwildcard = $(wildcard $1$2) $(foreach d,$(wildcard $1*),$(call rwildcard,$d/,$2))

CMN_DIR = common
CMN_SRC = $(wildcard $(CMN_DIR)/*.cs) $(wildcard deps/beef/src/*.cs)
BUILD_DIR = build

ASM_DIR = asm
ASM_EXE = bisc-asm.exe
ASM_SRC = $(wildcard $(ASM_DIR)/*.cs) $(wildcard $(ASM_DIR)/AssemblerData/*.cs) $(CMN_SRC)

VM_DIR = vm
VM_EXE = bisc-vm.exe
VM_SRC = $(wildcard $(VM_DIR)/*.cs) $(CMN_SRC)

CSC_FLAGS = -errorendlocation
FILE ?= programs/console

.PHONY: all
all: $(ASM_EXE) $(VM_EXE)

.PHONY: help
help:
	@echo "make     : build all make targets"
	@echo "make asm : run assembler"
	@echo "make vm  : run virtual machine"

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

clean:
	@rm -rf $(BUILD_DIR)
	@rm -rf $(ASM_DIR)/obj
	@rm -rf $(ASM_DIR)/bin
	@rm -rf $(VM_DIR)/obj
	@rm -rf $(VM_DIR)/bin
	@rm -f programs/*.exe
	@rm -f programs/*.bin
