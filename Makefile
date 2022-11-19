CMN_DIR = Common
CMN_SRC = $(wildcard $(CMN_DIR)/*.cs)
BUILD_DIR = Build

ASM_DIR = Assembler
ASM_EXE = bisc-asm.exe
ASM_SRC = $(wildcard $(ASM_DIR)/*.cs) $(CMN_SRC)

VM_DIR = VirtualMachine
VM_EXE = bisc-vm.exe
VM_SRC = $(wildcard $(VM_DIR)/*.cs) $(CMN_SRC) $(ASM_DIR)/Assembler.cs

.PHONY: all
all: $(ASM_EXE) $(VM_EXE)

$(ASM_EXE): $(ASM_SRC)
	mkdir -p $(BUILD_DIR)
	csc $(ASM_SRC) -out:$(BUILD_DIR)/$(ASM_EXE)

$(VM_EXE): $(VM_SRC)
	mkdir -p $(BUILD_DIR)
	csc $(VM_SRC) -out:$(BUILD_DIR)/$(VM_EXE)

asm: $(ASM_EXE)
	mono $(BUILD_DIR)/$(ASM_EXE)

vm: $(VM_EXE)
	mono $(BUILD_DIR)/$(VM_EXE)

clean:
	rm -r $(BUILD_DIR)