<template>
    <div class="agv_main">
        <div v-if="!showEdit">
            <div class="flex">
                <span style="font-size: 18px;">
                    协议列表
                </span>
                <el-button @click="addType" type="primary" :icon="Plus">添加协议</el-button>
            </div>
            <div class="agv_list">
                <div v-if="tableData.length > 0">
                    <el-row :gutter="15">
                        <el-col :span="6" v-for="data in tableData" :key="data.ID">
                            <el-card shadow="never" style="margin-bottom: 10px;">
                                <div class="flex">
                                    {{ data.Name }}
                                    <el-button-group>
                                        <el-button size="small" :icon="Close" @click="deleteRow(data)" />
                                        <el-button size="small" :icon="Edit" @click="edit(data)" />
                                    </el-button-group>
                                </div>
                                <el-text type="info">{{ showType(null, null, data.Type) }}</el-text>
                            </el-card>
                        </el-col>
                    </el-row>
                </div>
                <el-empty v-else />
            </div>
        </div>
        <div v-else>
            <InfoPage :form="form" @edit="resetForm" @submit="submitForm" />
        </div>
    </div>
</template>
<script setup>
// TODO 两个接口
// 编辑/修改，禁用/恢复
import { onMounted, ref, reactive } from "vue";
import { Plus, Close, Refresh, Edit } from '@element-plus/icons-vue'
import { getDeviceType, addDeviceType, editDeviceType } from "../utils/dotnet";
import InfoPage from "./info.vue";

const showEdit = ref(false)

const tableData = ref([]);
const form = reactive({
    ID: '',
    Name: '',
    Type: '',
    Read: [],
    Write: [],
    Config: []
})

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

onMounted(() => {
    // fetch("/dType.json").then(v => v.json()).then(data => {
    //      tableData.value = data;
    // });
    getDeviceType().then(data => {
        tableData.value = data;
    });
});
const showType = (row, col, type) => {
    for (let index = 0; index < options.length; index++) {
        const opt = options[index];
        if (opt.value == type[0])
            if (type.length > 1) {
                for (let i = 0; i < opt.children.length; i++) {
                    const children = opt.children[i];
                    if (children.value == type[1])
                        return opt.label + " / " + children.label;
                }
            } else
                return opt.label;
    }
}

const submitForm = (form) => {
    if (form.ID == "") {
        form.ID = crypto.randomUUID();
        tableData.value.push({ ...form });
        addDeviceType(form);

        //post("addType", { ...form })
    } else {
        var indexData = tableData.value.find((v, index, obj) => v.ID == form.ID);
        indexData.Name = form.Name;
        indexData.Type = form.Type;
        indexData.IsBig = form.IsBig;
        indexData.Read = form.Read.map(v=>({...v}));
        indexData.Write = form.Write.map(v=>({...v}));
        indexData.Config = form.Config.map(v=>({...v}));
        editDeviceType(indexData);
    }
    resetForm();
}
const setForm = (val) => {
    if (val) {
        form.ID = val.ID;
        form.Name = val.Name;
        form.Type = val.Type;
        form.IsBig = val.IsBig;
        form.Read = val.Read.map(v=>({...v}));
        form.Write = val.Write.map(v=>({...v}));
        form.Config = val.Config.map(v=>({...v}));
    } else {
        form.ID = "";
        form.Name = "";
        form.IsBig = false;
        form.Type = [];
        form.Read = [];
        form.Write = [];
        form.Config = [];
    }
}
const resetForm = () => {
    showEdit.value = false
    setForm();
}
const deleteRow = (row) => {
    var index = tableData.value.findIndex((v, index, obj) => v.ID = row.ID);
    tableData.value.splice(index, 1)
    resetForm();
}

const edit = (val) => {
    showEdit.value = true;
    setForm(val);
}
const addType = (val) => {
    showEdit.value = true;
}

</script>
<style scoped>
.agv_main {
    padding: 18px;
}

.flex {
    display: flex;
    justify-content: space-between;
    padding-bottom: 16px;
    align-items: center;
}

.page_header {
    padding-bottom: 16px;
    align-items: center;
}

/* .card-header{
    padding: 8px 12px
} */
</style>