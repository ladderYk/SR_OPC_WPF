<template>
    <div>
        <el-page-header class="page_header" @back="goBack">
            <template #content>
                <span class="text-large font-600 mr-3"> {{ form.ID ? "编辑" : "添加" }}协议 </span>
            </template>
        </el-page-header>
        <el-form :model="form" :inline="true" ref="formRef" label-width="auto">
            <el-form-item prop="Name" label="名称" :rules="[
                { required: true, message: '名称不能为空' }
            ]">
                <el-input v-model="form.Name" autocomplete="off" />
            </el-form-item>
            <el-form-item prop="Type" label="类型" :rules="[
                { required: true, message: '类型不能为空' }
            ]">
                <el-cascader v-model="form.Type" :options="options" />
            </el-form-item>
            <el-form-item prop="IsBig" label="大端">
                <el-switch v-model="form.IsBig" />
            </el-form-item>
        </el-form>
        <el-tabs>
            <el-tab-pane label="读取配置">
                <div>
                    <el-button size="small" :icon="Plus" :onclick="onAddItem1">添加</el-button>
                    <el-button size="small" :icon="Refresh" :onclick="onClearItem1">清空</el-button>
                </div>
                <el-table :data="form.Read" style="width: 100%;">
                    <el-table-column label="名称">
                        <template #default="scope">
                            <el-input v-model="scope.row.Name" autocomplete="off" />
                        </template>
                    </el-table-column>
                    <el-table-column label="标签">
                        <template #default="scope">
                            <el-input v-model="scope.row.Tag" autocomplete="off" />
                        </template>
                    </el-table-column>
                    <el-table-column label="地址">
                        <template #default="scope">
                            <el-input v-model="scope.row.Addr" autocomplete="off" />
                        </template>
                    </el-table-column>
                    <el-table-column label="长度">
                        <template #default="scope">
                            <el-input-number v-model="scope.row.Len" autocomplete="off" />
                        </template>
                    </el-table-column>
                    <el-table-column fixed="right" label="操作">
                        <template #default="scope">
                            <el-button size="small" :icon="Close" @click.prevent="deleteRow1(scope.$index)" />
                        </template>
                    </el-table-column>
                </el-table>
            </el-tab-pane>
            <el-tab-pane label="写入配置" name="second">
                <div>
                    <el-button size="small" :icon="Plus" :onclick="onAddItem2">添加</el-button>
                    <el-button size="small" :icon="Refresh" :onclick="onClearItem2">清空</el-button>
                </div>
                <el-table :data="form.Write" style="width: 100%;">
                    <el-table-column label="标签">
                        <template #default="scope">
                            <el-input v-model="scope.row.Tag" autocomplete="off" />
                        </template>
                    </el-table-column>
                    <el-table-column label="地址">
                        <template #default="scope">
                            <el-input v-model="scope.row.Addr" autocomplete="off" />
                        </template>
                    </el-table-column>
                    <el-table-column fixed="right" label="操作">
                        <template #default="scope">
                            <el-button size="small" :icon="Close" @click.prevent="deleteRow2(scope.$index)" />
                        </template>
                    </el-table-column>
                </el-table>
            </el-tab-pane>
            <el-tab-pane label="属性配置" name="3">
                <div>
                    <el-button size="small" :icon="Plus" :onclick="onAddItem3">生成</el-button>
                    <el-button size="small" :icon="Refresh" :onclick="onClearItem3">清空</el-button>
                </div>
                <el-table :data="form.Config" style="width: 100%;" @current-change="handleCurrentChange">
                    <el-table-column label="名称" prop="Addr" />
                    <el-table-column label="读取" prop="Name" />
                    <el-table-column label="偏移量" prop="Offset" />
                    <el-table-column label="类型">
                        <template #default="scope">
                            <el-select-v2 v-model="scope.row.DType" :options="options1" @change="onDtypeC"
                                v-if="!scope.row.Disable" />
                        </template>
                    </el-table-column>
                    <el-table-column label="标签">
                        <template #default="scope">
                            <el-input v-model="scope.row.Tag" autocomplete="off" v-if="!scope.row.Disable" />
                        </template>
                    </el-table-column>
                    <el-table-column fixed="right" label="操作">
                        <template #default="scope">
                            <el-button size="small" :icon="Close" @click.prevent="deleteRow3(scope.$index)" />
                        </template>
                    </el-table-column>
                </el-table>
            </el-tab-pane>
        </el-tabs>
        <div style="margin-top: 15px;">
            <el-button type="primary" @click="submitForm(formRef)">
                提交
            </el-button>
            <el-button @click="resetForm()">重置</el-button>
        </div>
    </div>
</template>
<script setup>
import { onMounted, ref, reactive } from "vue";
import { Plus, Close, Refresh, Edit } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'

const props = defineProps(['form'])
const emits = defineEmits(["edit", "submit"]);
const selProps = {
    label: 'name',
    value: 'tag',
};
const formRef = ref();
const tabCigRef = ref();
const cigRow = ref('');
const form = props.form;
const goBack = () => {
    emits("edit");
}
const options = [
    {
        value: 'Siemens',
        label: '西门子',
        children: [
            {
                value: 1,
                label: 'S1200',
            },
            {
                value: 2,
                label: 'S300',
            }
            , {
                value: 3,
                label: 'S400',
            }, {
                value: 4,
                label: 'S1500',
            }, {
                value: 5,
                label: 'S200Smart',
            }, {
                value: 6,
                label: 'S200',
            }
        ],
    }, {
        value: 'ModbusTcp',
        label: 'ModbusTcp'
    }];
const options1 = [
    {
        value: -1,
        label: '不设置',
    }, {
        value: 99,
        label: '结构体',
    },
    {
        value: 0,
        label: 'bool',
    }, {
        value: 1,
        label: 'byte'
    }, {
        value: 2,
        label: 'short'
    }, {
        value: 3,
        label: 'ushort'
    }, {
        value: 4,
        label: 'int'
    }, {
        value: 5,
        label: 'uint'
    }, {
        value: 6,
        label: 'long'
    }, {
        value: 7,
        label: 'ulong'
    }, {
        value: 8,
        label: 'float'
    }, {
        value: 9,
        label: 'double'
    }];
const tagOptions = ref([]);

const deleteRow1 = (index) => {
    form.Read.splice(index, 1)
}

const onAddItem1 = () => {
    form.Read.push({});
}
const onClearItem1 = () => {
    form.Read = [];
}

const deleteRow2 = (index) => {
    form.Write.splice(index, 1);
}

const onAddItem2 = () => {
    form.Write.push({});
}
const onClearItem2 = () => {
    form.Write = [];
}

const onAddItem3 = () => {
    var config = [];
    form.Read.forEach((read, index) => {
        if (read.Len == 1) {
            config.push({ ID: index + "", Addr: read.Addr, Name: read.Tag, Offset: 0 });
        }
        else if (read.Len > 1) {
            for (var i = 0; i < read.Len; i++) {
                config.push({ ID: index + "-" + i, Addr: i == 0 ? read.Addr : "", Name: read.Tag, Offset: i });
            }
        }
    })
    form.Config = config;
}
const onClearItem3 = () => {
    form.Config = [];
}
const deleteRow3 = (index) => {
    form.Config.splice(index, 1);
}

const submitForm = (formEl) => {
    if (!formEl) return
    formEl.validate((valid) => {
        if (valid) {
            ElMessage({
                message: (form.ID ? "编辑" : "添加") + "成功！",
                type: 'success',
            })
            emits("submit", { ...form })
        }
    });
}
const resetForm = () => {
    form.Name = "";
    form.IsBig = false;
    form.Type = [];
    form.Read = [];
    form.Write = [];
    form.Config = [];
}
const onDtypeC = val => {
    if (cigRow.value != null) {
        var rowIds = cigRow.value.ID.split("-");
        var oVal = cigRow.value.DType;
        if (oVal > 1) {
            var oLen = 1;
            if (oVal == 2 || oVal == 3) {
                oLen = 2;
            }
            else if (oVal == 4 || oVal == 5) {
                oLen = 4;
            }
            if (oLen > 1) {
                form.Config.forEach(v => {
                    var cIds = v.ID.split("-");
                    if (rowIds[0] == cIds[0] && (rowIds[1] - 0) + oLen > cIds[1] - 0 && cIds[1] > rowIds[1]) {
                        v.Disable = false;
                    }
                });
            }
        }
        var len = 1;
        if (val == 2 || val == 3) {
            len = 2;
        }
        else if (val == 4 || val == 5) {
            len = 4;
        }
        if (len > 1) {
            form.Config.forEach(v => {
                var cIds = v.ID.split("-");
                if (rowIds[0] == cIds[0] && (rowIds[1] - 0) + len > cIds[1] - 0 && cIds[1] > rowIds[1]) {
                    v.DType = -1;
                    v.Disable = true;
                }
            });
            cigRow.value.DType = val;
        }

    }
}
const handleCurrentChange = row => {
    cigRow.value = { ...row };
}
</script>
<style scoped>
.page_header {
    padding-bottom: 16px;
    align-items: center;
}
</style>