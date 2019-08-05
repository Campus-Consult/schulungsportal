<template>
    <div class="form-horizontal">
        <div class="form-group hidden-print">
            <label class="col-md-2 control-label">Vorname:</label><div class="col-md-4"><input class="form-control text-box single-line" v-model="vorname"></div>
            <label class="col-md-2 control-label">Nachname:</label><div class="col-md-4"><input class="form-control text-box single-line" v-model="nachname"></div>
            <label class="col-md-2 control-label">E-Mail:</label><div class="col-md-4"><input class="form-control text-box single-line" v-model="email"></div>
            <label class="col-md-2 control-label">Handynummer:</label><div class="col-md-4"><input class="form-control text-box single-line" v-model="handynummer"></div>
        </div>
            <div class="form-group hidden-print" style="height: 1em;">
                <span v-if="searching">Lade...</span>
            </div>
            <div class="form-group">
                {{errorMessage}}
            </div>
            <div v-if="anmeldungen.length">
                <div class="form-group hidden-print">
                    <button v-bind:disabled="!anyselected" @click="deleteSelected" class="btn btn-danger">Teilnehmer anonymisieren</button>
                    <button @click="toggleSchulungView" class="btn btn-default">{{showSchulungViewText}}</button>
                    <button v-bind:disabled="!anyselected" @click="printSelected" class="btn btn-default">Historie drucken</button>
                </div>
                <div class="form-group hidden-print">
                    <input type="checkbox" v-model="showSelectedOnly">Nur ausgewählte anzeigen
                </div>
                <div class="form-group">
                    <h2>Suchergebnisse für {{fullname}}</h2>
                    <table class="table" v-bind:class="{'hidden' : showSchulungView}">
                        <tbody>
                            <tr>
                                <th class="hidden-print"><input type="checkbox" v-model="selectAll"></th>
                                <th>Match Count</th>
                                <th>Vorname</th>
                                <th>Nachname</th>
                                <th>Email</th>
                                <th>Telefonnummer</th>
                            </tr>
                            <tr v-bind:key="anmeldung.AnmeldungID" v-for="anmeldung in anmeldungen" v-bind:class="{ 'hidden': !anmeldung.checked && showSelectedOnly,'hidden-print': !anmeldung.checked }">
                                <td class="hidden-print"><input type="checkbox" @change="checkAllChecked" v-model="anmeldung.checked"></td>
                                <td>{{anmeldung.matchCount}}</td>
                                <td>{{anmeldung.vorname}}</td>
                                <td>{{anmeldung.nachname}}</td>
                                <td>{{anmeldung.eMail}}</td>
                                <td>{{anmeldung.handynummer}}</td>
                            </tr>
                        </tbody>
                    </table>
                    <table class="table" v-bind:class="{'hidden' : !showSchulungView}">
                        <tbody>
                            <tr>
                                <th>Titel</th>
                                <th>Organisation</th>
                                <th>Start-Termin</th>
                                <th>End-Termin</th>
                                <th></th>
                            </tr>
                            <tr v-bind:key="anmeldung.AnmeldungID" v-for="anmeldung in anmeldungen" v-bind:class="{ 'hidden': !anmeldung.checked && showSelectedOnly,'hidden-print': !anmeldung.checked }">
                                <td>{{anmeldung.schulung.titel}}</td>
                                <td>{{anmeldung.schulung.organisatorInstitution}}</td>
                                <td v-html="formatTermineStart(anmeldung.schulung.termine)"></td>
                                <td v-html="formatTermineEnd(anmeldung.schulung.termine)"></td>
                                <td class="hidden-print"><a v-bind:href="'/Schulung/Details/'+anmeldung.schulungGUID" target="_blank">Details</a></td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
</template>
<script lang="ts">
import { Component, Vue , Watch} from "vue-property-decorator";
import VuejsDialog from "vuejs-dialog";
import VuejsDialogMixin from 'vuejs-dialog/dist/vuejs-dialog-mixin.min.js'; // only needed in custom components

import 'vuejs-dialog/dist/vuejs-dialog.min.css';

import {Termin, Schulung, AnmeldungWithMatchCount, SucheRequest, SucheApi, AnmeldungApi} from './api';

// Tell Vue to install the plugin.
Vue.use(VuejsDialog);

interface AnmeldungWithMatchCountCheck extends AnmeldungWithMatchCount {
    checked: boolean,
}

@Component({})
export default class TeilnehmerSuche extends Vue {
    vorname = "";
    nachname = "";
    email = "";
    handynummer = "";
    anmeldungen: Array<AnmeldungWithMatchCountCheck> = [];
    sucheTimeout: number;
    searching: boolean = false;
    selectAll: boolean = false;
    showSelectedOnly: boolean = false;
    showSchulungView: boolean = false;
    errorMessage: string = "";
    $dialog: any;

    // --- computed properties
    // dummy computed property to trigger the search on any input
    get alltogether(): string {
        return this.vorname+this.nachname+this.email+this.handynummer;
    }

    get anyselected(): boolean {
        return this.anmeldungen.some(a => a.checked);
    }

    get fullname(): string {
        if (this.vorname && this.nachname) {
            return this.vorname + " " + this.nachname;
        } else {
            // at least one is empty so no unnecessary space
            return this.vorname + this.nachname;
        }
    }

    get shownAnmeldungen(): Array<AnmeldungWithMatchCountCheck> {
        if (this.showSelectedOnly) {
            return this.anmeldungen.filter(a => a.checked);
        } else {
            return this.anmeldungen;
        }
    }

    get showSchulungViewText(): string {
        if (this.showSchulungView) {
            return "Anmeldungsansicht";
        } else {
            return "Schulungsansicht";
        }
    }

    // --- watchers

    @Watch('alltogether')
    debounceSearch() {
        clearTimeout(this.sucheTimeout);
        this.sucheTimeout = setTimeout(this.doSearch, 500);
        this.searching = true;
    }

    @Watch('selectAll')
    selectAllChange(newVal: boolean) {
        this.anmeldungen.forEach(a => a.checked = newVal);
    }

    // -- events

    checkAllChecked() {
        if (this.anmeldungen.length > 0) {
            // if they are all either checked or unchecked change the toggle for all
            let maybeUnique = this.anmeldungen[0].checked;
            if (this.anmeldungen.every(a => a.checked == maybeUnique)) {
                this.selectAll = maybeUnique;
            }
        }
    }

    deleteSelected() {
        const toDelete = this.anmeldungen.filter(a => a.checked).map(a => a.anmeldungID);
        this.$dialog.confirm(`Alle ${toDelete.length} ausgewählten Anmeldungen anonymisieren?`, {loader: true})
            .then(dialog => {
                AnmeldungApi.anonymize(toDelete)
                    .then(() => {
                        // remove deleted entries
                        this.anmeldungen = this.anmeldungen.filter(a => !toDelete.includes(a.anmeldungID));
                        dialog.close();
                    })
                    .catch(e => {
                        // TODO proper error handling
                        console.log(e);
                        this.errorMessage = "Fehler beim Löschen der Teilnehmer!";
                        dialog.close();
                    })
            })
            .catch(()=>{});
    }

    printSelected() {
        window.print();
    }

    toggleSchulungView() {
        this.showSchulungView = !this.showSchulungView;
    }

    // -- helpers

    doSearch() {
        SucheApi.teilnehmer({vorname: this.vorname, nachname: this.nachname, email: this.email, handynummer: this.handynummer})
            .then(r => {
                this.anmeldungen = r.data.map(a => {var ac = a as AnmeldungWithMatchCountCheck; ac.checked = false; return ac;}).sort((a,b)=>b.matchCount-a.matchCount);
                console.log(this.anmeldungen);
            })
            .catch(e => {
                console.log(e);
                this.errorMessage = "Fehler beim Suchen der Teilnehmer!";
            });
        this.searching = false;
    }

    formatDate(date: string): string {
        return new Date(date).toLocaleString();
    }

    formatTermineStart(termine: Array<Termin>): string {
        return termine.map(t => this.formatDate(t.start)).join('</br>');
    }

    formatTermineEnd(termine: Array<Termin>): string {
        return termine.map(t => this.formatDate(t.end)).join('</br>');
    }
}
</script>
<style>
    .dg-btn--cancel {
        background-color: rgb(24,62,116);
    }
    .dg-btn--ok {
        border-color: red;
        color: red;
    }
</style>
