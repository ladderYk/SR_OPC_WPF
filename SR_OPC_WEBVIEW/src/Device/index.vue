<template>
    <div class="agv_main">
        <div class="flex">
            <span style="font-size: 18px;">
                设备列表
            </span>
            <el-button @click="showAddDevice" type="primary" :icon="Plus">添加设备</el-button>
        </div>
        <div class="agv_list">
            <el-scrollbar v-if="tableData.length > 0" height="400px">
                <div style="overflow: hidden;">
                    <el-row :gutter="15">
                        <el-col :span="6" v-for="data in tableData" :key="data.ID">
                            <el-card shadow="never" style="margin-bottom: 10px;">
                                <el-row>
                                    <el-col :span="20">
                                        <div class="flex">
                                            {{ data.Name }}
                                            <el-button-group>
                                                <el-button size="small" :icon="Close" @click="deleteRowN(data)" />
                                                <el-button size="small" :icon="Edit" @click="edit(data)" />
                                                <el-button size="small" :icon="View" @click="cardClick(data)"
                                                    :type="rowID == data.ID ? 'primary' : ''" />
                                            </el-button-group>
                                        </div>
                                        <el-text type="info">{{ data.Model }}</el-text>
                                    </el-col>
                                    <el-col :span="4">
                                        <div class="flex-center">
                                            <div class="bow" :class="data.Online ? 'online' : 'offline'"></div>
                                        </div>
                                    </el-col>
                                </el-row>
                            </el-card>
                        </el-col>
                    </el-row>
                </div>
            </el-scrollbar>
            <el-divider />
            <InfoPage :form="row" v-if="rowID != ''" />
        </div>
    </div>
    <el-drawer v-model="dialogFormVisible" width="400" destroy-on-close :close-on-click-modal="false">
        <template #header="{ titleId, titleClass }">
            <div :id="titleId" :class="titleClass">
                {{ form.ID == "" ? "添加" : "编辑" }}
            </div>
        </template>
        <el-form :model="form" label-position="top" ref="formRef" label-width="auto">
            <el-form-item prop="Name" label="名称" :rules="[
                { required: true, message: '名称不能为空' }
            ]">
                <el-input v-model="form.Name" autocomplete="off" />
            </el-form-item>
            <el-form-item prop="Model" label="类型" :rules="[
                { required: true, message: '类型不能为空' }
            ]">
                <el-select-v2 :props="props" :options="options" v-model="form.Model" value-key="Name" />
            </el-form-item>
            <el-form-item prop="IP" label="IP" :rules="[
                { required: true, message: 'IP不能为空' }
            ]">
                <el-input v-model="form.IP" autocomplete="off" />
            </el-form-item>
            <el-form-item prop="Port" label="端口号" :rules="[
                { required: true, message: '端口号不能为空' }
            ]">
                <el-input v-model="form.Port" autocomplete="off" />
            </el-form-item>
            <el-form-item prop="Timeout" label="连接超时时间" :rules="[
                { required: true, message: '连接超时时间不能为空' }
            ]">
                <el-input v-model="form.Timeout" autocomplete="off" />
            </el-form-item>
            <el-form-item prop="Cycle" label="循环周期" :rules="[
                { required: true, message: '循环周期不能为空' }
            ]">
                <el-input v-model="form.Cycle" autocomplete="off" />
            </el-form-item>
        </el-form>
        <template #footer>
            <div>
                <el-button type="primary" @click="submitForm(formRef)">
                    提交
                </el-button>
                <el-button @click="dialogFormVisible = false">取消</el-button>
            </div>
        </template>
    </el-drawer>
</template>
<script setup>
// TODO 两个接口
// 编辑/修改，禁用/恢复
import { onMounted, ref, reactive, onUnmounted } from "vue";
import { Close, Edit, Plus, View } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import InfoPage from "./info.vue";
import { getDeviceList, getDeviceType, editDevice, addDevice } from "../utils/dotnet";

let sokect = null;
const dialogFormVisible = ref(false)

const formRef = ref();
const tableData = ref([]);
const rowID = ref("");
const row = reactive({
    Name: ""
});

const form = reactive({
    ID: "",
    Name: "",
    Model: "",
    IP: "",
    Port: 102,
    Timeout: 1000,
    Cycle: 1000
});
const props = {
    label: 'Name',
    value: 'Name',
}
const options = ref([]);
onMounted(() => {
    // get("/DeviceList").then(data => {
    //     tableData.value = data;
    // });
    getDeviceList().then(data => {
        tableData.value = data;
    });
    getDeviceType().then(data => {
        options.value = data;
    });

    sokect = new WebSocket(ws);
    sokect.onopen = () => {
        sokect.send(JSON.stringify({ topic: 'online', action: 'subscribe' }));

        //sokect.send(JSON.stringify({ action: 'subscribe', data: 'agvList' }));
    };
    sokect.onerror = () => {
        // setTimeout(() => {
        //   document.location.reload();
        // }, 10000);
    }
    sokect.onmessage = (message) => {
        const jmessage = JSON.parse(message.data);
        var agv = tableData.value.find((v, index, obj) => v.Name == jmessage.name);
        if (agv == null)
            return;
        if (agv.Online != jmessage.online) {
            agv.Online = jmessage.online;
        }
    };
});
const showAddDevice = () => {
    dialogFormVisible.value = true;
    resetForm();
}
const edit = (val) => {
    dialogFormVisible.value = true;
    setForm(val);
}
const setForm = (val) => {
    if (val) {
        form.ID = val.ID;
        form.Name = val.Name;
        form.Model = val.Model;
        form.IP = val.IP;
        form.Port = val.Port;
        form.Timeout = val.Timeout;
        form.Cycle = val.Cycle;
    } else {
        form.ID = "";
        form.Name = "";
        form.Model = "";
        form.IP = "";
        form.Port = 102;
        form.Timeout = 1000;
        form.Cycle = 1000;
    }
}
const submitForm = (formEl) => {
    if (!formEl) return
    formEl.validate((valid) => {
        if (valid) {
            ElMessage({
                message: (form.ID ? "编辑" : "添加") + "成功！",
                type: 'success',
            })
            if (form.ID == "") {
                form.ID = crypto.randomUUID();
                tableData.value.push({ ...form });
                addDevice(form);
            } else {
                var indexData = tableData.value.find((v, index, obj) => v.ID == form.ID);;
                indexData.Name = form.Name;
                indexData.Model = form.Model;
                indexData.IP = form.IP;
                indexData.Port = form.Port;
                indexData.Timeout = form.Timeout;
                indexData.Cycle = form.Cycle;
                editDevice(indexData);
            }
            dialogFormVisible.value = false;
            resetForm();
        } else {
            return false;
        }
    })
}
const resetForm = () => {
    setForm();
}
const deleteRow = (index) => {
    tableData.value.splice(index, 1)
    resetForm();
}
const deleteRowN = (row) => {
    var index = tableData.value.findIndex((v, index, obj) => v.ID = row.ID);
    tableData.value.splice(index, 1)
    resetForm();
}

const cardClick = data => {
    rowID.value = data.ID;
    row.Name = data.Name;
}
onUnmounted(() => {
    sokect.send(JSON.stringify({ topic: 'online', action: 'unsubscribe' }));
    sokect.close(1000);
});

</script>
<style scoped>
.agv_main {
    padding: 18px;
}

.flex {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding-bottom: 16px;
}

.flex-center {
    display: flex;
    justify-content: center;
}

.bow {
    width: 25px;
    height: 25px;
    border-radius: 50%;
}

.online {
    background-color: green;

}

.offline {
    background-color: red;
}

/* .card-header{
    padding: 8px 12px
} */
</style>